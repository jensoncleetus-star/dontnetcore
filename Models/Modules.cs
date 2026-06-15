using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace QuickSoft.Models
{
    public class AppModules : IdentityRole
    {
        public AppModules()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public AppModules(string name)
        {
            this.Name = name;
        }
        public long ModulesID { get; set; }

        [StringLength(100)]
        [Display(Name = "Menu Name")]
        public string viewName { get; set; }

        [StringLength(250)]
        public string Link { get; set; }

        [StringLength(100)]
        [Display(Name = "Menu Icon")]
        public string iconClass { get; set; }

        public long Parent { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Make as parent")]
        public choice IsParent { get; set; } = choice.No;

        [Display(Name = "Add to Menu")]
        public choice addMenu { get; set; } = choice.Yes;

        public String Employee { get; set; }
        public Status Status { get; set; } = Status.active;
        public choice Editable { get; set; } = choice.Yes;
        [Display(Name = "Menu Order")]
        public int MenuOrder { get; set; } = 0;

    }
    public class Sections
    {
        public long Id { get; set; }
        [StringLength(100)]
        public String Name { get; set; }
    }
    public class SectionModules
    {
        public long Id { get; set; }
        public long SectionId { get; set; }
        public long ModulesID { get; set; }
    }

    // role group
    #region rolegroup
    public class RoleGroup
    {
        public RoleGroup()
        {
            this.Id = Guid.NewGuid().ToString();
            this.GroupModules = new List<RoleGroupModule>();
            this.UserGroup = new List<UserGroup>();
        }

        public RoleGroup(string name) : this()
        {
            this.Name = name;
        }

        public RoleGroup(string name, string description) : this(name)
        {
            this.Description = description;
        }

        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<RoleGroupModule> GroupModules { get; set; }
        public virtual ICollection<UserGroup> UserGroup { get; set; }
    }

    // user rolegroup link
    public class UserGroup
    {
        public string UserId { get; set; }
        public string RoleGroupId { get; set; }
    }


    public class RoleGroupModule
    {
        public string RoleGroupId { get; set; }
        public string ModuleId { get; set; }
    }
    public class ApplicationRoleStore : RoleStore<AppModules, ApplicationDbContext>
    {
        public ApplicationRoleStore(ApplicationDbContext context) : base(context)
        {
        }
    }
    #endregion
}