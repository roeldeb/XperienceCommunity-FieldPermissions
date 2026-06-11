using System.Text.Json;

using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites.Internal;

using Kentico.Xperience.Admin.Base.Forms;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using XperienceCommunity.FieldPermissions.Services;

namespace XperienceCommunity.FieldPermissions.Extenders;

/// <summary>
/// Base class for role-aware form component extenders.
/// Checks field-level permissions and applies restrictions (disable or hide) for users without the required roles.
/// </summary>
/// <remarks>
/// XbyK matches extenders by exact form component type — there is no inheritance-based matching.
/// Each built-in form component type requires its own one-liner extender class deriving from this base.
/// For custom or third-party form components, consumers register their own extender:
/// <code>
/// [assembly: FormComponentExtender(typeof(RoleAwareMyCustomExtender))]
/// public sealed class RoleAwareMyCustomExtender()
///     : RoleAwareFormComponentExtenderBase&lt;MyCustomComponent&gt;();
/// </code>
/// </remarks>
/// <typeparam name="T">The form component type to extend.</typeparam>
public abstract class RoleAwareFormComponentExtenderBase<T>
    : FormComponentExtender<T> where T : IFormComponent
{
    // The extender runs once per form component, but create mode and the edited content type are facts of
    // the request, not the field. They are memoized in HttpContext.Items so the resolution (JSON parsing,
    // Referer/path parsing and the content item / web page database lookups) runs once per request rather
    // than once per field.
    private const string ContentTypeIdItemKey = "XperienceCommunity.FieldPermissions.ContentTypeId";
    private const string CreateModeItemKey = "XperienceCommunity.FieldPermissions.IsCreateMode";

    private static HttpContext? HttpContext => Service.Resolve<IHttpContextAccessor>().HttpContext;

    /// <summary>
    /// Resolves a service from the current HTTP request's scoped service provider.
    /// <c>Service.Resolve&lt;T&gt;()</c> uses the root provider which cannot resolve scoped services.
    /// </summary>
    private static TService? GetScopedService<TService>() where TService : class
        => HttpContext?.RequestServices.GetService<TService>();

    /// <summary>
    /// Memoizes a request-level value in <see cref="HttpContext.Items"/>, computing it via <paramref name="factory"/>
    /// on first access. Falls back to computing the value directly when there is no current HTTP context.
    /// </summary>
    private static TValue GetOrAddRequestValue<TValue>(string key, Func<TValue> factory)
    {
        var httpContext = HttpContext;

        if (httpContext is null)
        {
            return factory();
        }

        if (httpContext.Items.TryGetValue(key, out object? cached))
        {
            return (TValue)cached!;
        }

        var value = factory();
        httpContext.Items[key] = value;
        return value;
    }

    /// <inheritdoc/>
    public override async Task ConfigureComponent()
    {
        var fieldPermissionService = GetScopedService<IFieldPermissionService>();
        if (fieldPermissionService is null)
        {
            return;
        }

        int? contentTypeId = GetContentTypeId();

        var restriction = await fieldPermissionService.GetFieldRestrictionAsync(FormComponent.Guid, contentTypeId);

        if (restriction is null)
        {
            return;
        }

        // When creating a new item, a field flagged to show in create mode bypasses all restrictions.
        if (restriction.ShowInCreateMode && IsCreateMode())
        {
            return;
        }

        switch (restriction.Mode)
        {
            case FieldPermissionMode.Disable:
                if (FormComponent.Properties is FormComponentProperties props)
                {
                    props.EditMode = FormEditMode.Disabled;

                    if (!string.IsNullOrEmpty(restriction.InactiveMessage))
                    {
                        props.InactiveMessage = restriction.InactiveMessage;
                    }
                }
                break;

            case FieldPermissionMode.Hide:
                FormComponent.AddVisibilityCondition(new AlwaysHiddenVisibilityCondition());
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported {nameof(FieldPermissionMode)} value: {restriction.Mode}.");
        }
    }

    /// <summary>
    /// The DataClassInfo.ClassID of the content type being edited or created, resolved once per request.
    /// </summary>
    private static int? GetContentTypeId()
        => GetOrAddRequestValue(ContentTypeIdItemKey, TryResolveContentTypeId);

    /// <summary>
    /// Determines whether the form is in create mode (new item) vs edit mode (existing item), resolved once
    /// per request.
    /// Uses the Referer header because extender <c>ConfigureComponent()</c> runs during component activation
    /// (in <c>FormComponentFromFormFieldActivator</c>), before <c>BindContext()</c> sets the <c>FormContext</c>.
    /// All XbyK admin create pages use a <c>/create</c> URL suffix.
    /// </summary>
    private static bool IsCreateMode()
        => GetOrAddRequestValue(CreateModeItemKey, ResolveCreateMode);

    private static bool ResolveCreateMode()
    {
        string? referer = HttpContext?.Request.Headers.Referer.ToString();

        if (string.IsNullOrEmpty(referer) || !Uri.TryCreate(referer, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.AbsolutePath.EndsWith("/create", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resolves the DataClassInfo.ClassID of the content type currently being edited or created.
    /// </summary>
    /// <remarks>
    /// Required to disambiguate fields shared across content types via reusable field schemas, which carry
    /// the same field GUID. The content type is not available on the component itself (the extender runs
    /// before <c>BindContext()</c>), so it is derived from the admin request:
    /// <list type="number">
    /// <item>The posted command <c>data</c> JSON, which carries the chosen content type for create flows
    /// (<c>contentTypeId</c>) and content item form commands (<c>formName</c> = <c>"prefix:contentTypeId"</c>).</item>
    /// <item>For edit, the object identifier in the page path (web page item or reusable content item),
    /// resolved to its content type. This is skipped in create mode, where the path's web page segment is
    /// the <em>parent</em> page, not the new item.</item>
    /// </list>
    /// Returns <c>null</c> when the content type cannot be determined (e.g. module class forms, whose field
    /// GUIDs are unique and therefore need no disambiguation).
    /// </remarks>
    private static int? TryResolveContentTypeId()
    {
        if (TryGetContentTypeIdFromCommandData() is { } idFromData)
        {
            return idFromData;
        }

        // In create mode the path's web page segment is the parent page, so path resolution would be wrong.
        if (IsCreateMode())
        {
            return null;
        }

        return TryResolveContentTypeIdFromPath();
    }

    /// <summary>
    /// Reads the content type id from the posted command <c>data</c> JSON: a numeric <c>contentTypeId</c>
    /// property, or a <c>formName</c> of the form <c>"prefix:contentTypeId"</c>.
    /// </summary>
    private static int? TryGetContentTypeIdFromCommandData()
    {
        var request = HttpContext?.Request;

        if (request is null || !request.HasFormContentType || !request.Form.TryGetValue("data", out var dataValues))
        {
            return null;
        }

        string data = dataValues.ToString();

        if (string.IsNullOrEmpty(data))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(data);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Number
                    && string.Equals(property.Name, "contentTypeId", StringComparison.OrdinalIgnoreCase)
                    && property.Value.TryGetInt32(out int contentTypeId)
                    && contentTypeId > 0)
                {
                    return contentTypeId;
                }

                if (property.Value.ValueKind == JsonValueKind.String
                    && string.Equals(property.Name, "formName", StringComparison.OrdinalIgnoreCase)
                    && ParseContentTypeIdFromFormName(property.Value.GetString()) is { } idFromFormName)
                {
                    return idFromFormName;
                }
            }
        }
        catch (JsonException)
        {
            // Malformed or unexpected command data — fall through to path-based resolution.
        }

        return null;
    }

    /// <summary>
    /// Parses the trailing content type id from a form name of the form <c>"prefix:contentTypeId"</c>.
    /// </summary>
    private static int? ParseContentTypeIdFromFormName(string? formName)
    {
        if (string.IsNullOrEmpty(formName))
        {
            return null;
        }

        int separatorIndex = formName.LastIndexOf(':');

        return separatorIndex >= 0
            && int.TryParse(formName[(separatorIndex + 1)..], out int contentTypeId)
            && contentTypeId > 0
                ? contentTypeId
                : null;
    }

    /// <summary>
    /// Resolves the content type from an object identifier in the current page path: a web page item
    /// (path segment <c>"{language}_{webPageItemId}"</c>) or a reusable content item (a bare integer segment).
    /// </summary>
    private static int? TryResolveContentTypeIdFromPath()
    {
        string? path = GetPagePath();

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Website channels: a "{language}_{webPageItemId}" segment (e.g. "en_42", "en-US_42").
        foreach (string segment in segments)
        {
            int separatorIndex = segment.LastIndexOf('_');

            if (separatorIndex > 0
                && int.TryParse(segment[(separatorIndex + 1)..], out int webPageItemId)
                && webPageItemId > 0
                && ResolveContentTypeFromWebPageItem(webPageItemId) is { } contentTypeId)
            {
                return contentTypeId;
            }
        }

        // Content hub: a bare integer segment mapping to a reusable content item. The content item id
        // appears late in the path, so scan from the end to avoid earlier ids (workspace, folder).
        for (int i = segments.Length - 1; i >= 0; i--)
        {
            if (int.TryParse(segments[i], out int contentItemId)
                && contentItemId > 0
                && ResolveContentTypeFromReusableContentItem(contentItemId) is { } contentTypeId)
            {
                return contentTypeId;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the page path of the current admin request: the posted <c>path</c> field when present,
    /// otherwise the path of the <c>Referer</c> header.
    /// </summary>
    private static string? GetPagePath()
    {
        var request = HttpContext?.Request;

        if (request is null)
        {
            return null;
        }

        if (request.HasFormContentType
            && request.Form.TryGetValue("path", out var pathValues)
            && pathValues.ToString() is { Length: > 0 } postedPath)
        {
            return postedPath;
        }

        string referer = request.Headers.Referer.ToString();

        return !string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var uri)
            ? uri.AbsolutePath
            : null;
    }

    private static int? ResolveContentTypeFromWebPageItem(int webPageItemId)
    {
        var webPageItem = Service.Resolve<IInfoProvider<WebPageItemInfo>>().Get(webPageItemId);

        if (webPageItem is null)
        {
            return null;
        }

        var contentItem = Service.Resolve<IInfoProvider<ContentItemInfo>>().Get(webPageItem.WebPageItemContentItemID);

        return contentItem?.ContentItemContentTypeID;
    }

    private static int? ResolveContentTypeFromReusableContentItem(int contentItemId)
    {
        var contentItem = Service.Resolve<IInfoProvider<ContentItemInfo>>().Get(contentItemId);

        // Only reusable items belong to the content hub; this also rules out web page content items
        // and unrelated identifiers (workspace, folder) that happen to be integers in the path.
        return contentItem is { ContentItemIsReusable: true }
            ? contentItem.ContentItemContentTypeID
            : null;
    }
}
