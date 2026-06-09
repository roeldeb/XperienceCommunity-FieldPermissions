using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.FieldPermissions.Classes;

[assembly: RegisterObjectType(typeof(FieldPermissionInfo), FieldPermissionInfo.OBJECT_TYPE)]

namespace XperienceCommunity.FieldPermissions.Classes
{
    /// <summary>
    /// Data container class for <see cref="FieldPermissionInfo"/>.
    /// </summary>
    public partial class FieldPermissionInfo : AbstractInfo<FieldPermissionInfo, IInfoProvider<FieldPermissionInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "xperiencecommunity.fieldpermission";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<FieldPermissionInfo>), OBJECT_TYPE, "XperienceCommunity.FieldPermission", "FieldPermissionID", null, "FieldPermissionGuid", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true,
            },
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("FieldPermissionContentTypeID", "cms.class", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Field permission ID.
        /// </summary>
        [DatabaseField]
        public virtual int FieldPermissionID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FieldPermissionID)), 0);
            set => SetValue(nameof(FieldPermissionID), value);
        }


        /// <summary>
        /// Field permission guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid FieldPermissionGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(FieldPermissionGuid)), Guid.Empty);
            set => SetValue(nameof(FieldPermissionGuid), value);
        }


        /// <summary>
        /// Field permission content type ID.
        /// </summary>
        [DatabaseField]
        public virtual int FieldPermissionContentTypeID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FieldPermissionContentTypeID)), 0);
            set => SetValue(nameof(FieldPermissionContentTypeID), value);
        }


        /// <summary>
        /// Field permission field guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid FieldPermissionFieldGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(FieldPermissionFieldGuid)), Guid.Empty);
            set => SetValue(nameof(FieldPermissionFieldGuid), value);
        }


        /// <summary>
        /// Field permission role mode (allow or disallow).
        /// </summary>
        [DatabaseField]
        public virtual string FieldPermissionRoleMode
        {
            get => ValidationHelper.GetString(GetValue(nameof(FieldPermissionRoleMode)), "allow");
            set => SetValue(nameof(FieldPermissionRoleMode), value);
        }


        /// <summary>
        /// Field permission roles.
        /// </summary>
        [DatabaseField]
        public virtual string FieldPermissionAllowedRoles
        {
            get => ValidationHelper.GetString(GetValue(nameof(FieldPermissionAllowedRoles)), String.Empty);
            set => SetValue(nameof(FieldPermissionAllowedRoles), value);
        }


        /// <summary>
        /// Field permission mode.
        /// </summary>
        [DatabaseField]
        public virtual int FieldPermissionMode
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FieldPermissionMode)), 0);
            set => SetValue(nameof(FieldPermissionMode), value);
        }


        /// <summary>
        /// Field permission inactive message.
        /// </summary>
        [DatabaseField]
        public virtual string FieldPermissionInactiveMessage
        {
            get => ValidationHelper.GetString(GetValue(nameof(FieldPermissionInactiveMessage)), String.Empty);
            set => SetValue(nameof(FieldPermissionInactiveMessage), value, String.Empty);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="FieldPermissionInfo"/> class.
        /// </summary>
        public FieldPermissionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FieldPermissionInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FieldPermissionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
