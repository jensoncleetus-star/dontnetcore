using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class IdentityManager
    {

        //private ApplicationGroupStore _groupStore;
        // Swap ApplicationRole for IdentityRole:
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        private readonly RoleManager<AppModules> _roleManager = new RoleManager<AppModules>(
            new RoleStore<AppModules>(new ApplicationDbContext()));

        private readonly UserManager<ApplicationUser> _userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(new ApplicationDbContext()));



        public IdentityManager()
        {
            _userManager = LegacyWeb.Current
                .GetOwinContext().GetUserManager<ApplicationUserManager>();
            //_roleManager = LegacyWeb.Current.GetOwinContext().Get<ApplicationRoleManager>();
            //_groupStore = new ApplicationGroupStore(_db);
        }
        public bool RoleExists(string name)
        {
            return _roleManager.RoleExists(name);
        }

        public IdentityResult CreateRole(AppModules modl)
        {
            // Swap ApplicationRole for IdentityRole:
            return _roleManager.Create(modl);
        }
        public IdentityResult UpdateRole(AppModules modl)
        {
            // Swap ApplicationRole for IdentityRole:
            return _roleManager.Update(modl);
        }

        public IdentityResult CreateUser(ApplicationUser user, string password)
        {
            return _userManager.Create(user, password);
        }

        public IdentityResult AddUserToRole(string userId, string roleName)
        {
            return _userManager.AddToRole(userId, roleName);
        }


        public void ClearUserRoles(string userId)
        {
            ApplicationUser user = _userManager.FindById(userId);
            var currentRoles = new List<IdentityUserRole>();

            currentRoles.AddRange(user.Roles);
            foreach (IdentityUserRole role in currentRoles)
            {
                _userManager.RemoveFromRole(userId, role.RoleId);
            }
        }

        public void RemoveFromRole(string userId, string roleName)
        {
            _userManager.RemoveFromRole(userId, roleName);
        }

        public void DeleteRole(string roleId)
        {
            IQueryable<ApplicationUser> roleUsers = _db.Users.Where(u => u.Roles.Any(r => r.RoleId == roleId));
            AppModules role = _db.AppModuless.Find(roleId);

            foreach (ApplicationUser user in roleUsers)
            {
                RemoveFromRole(user.Id, role.Name);
            }
            _db.AppModuless.Remove(role);
            _db.SaveChanges();
        }

        public void CreateGroup(string groupName)
        {
            if (GroupNameExists(groupName))
            {
                throw new GroupExistsException(
                    "A group by that name already exists in the database. Please choose another name.");
            }

            var newGroup = new RoleGroup(groupName);
            _db.RoleGroups.Add(newGroup);
            _db.SaveChanges();
        }

        public bool GroupNameExists(string groupName)
        {
            return _db.RoleGroups.Any(gr => gr.Name == groupName);
        }

        //public async Task<IdentityResult> CreateGroupAsync(RoleGroup group)
        //{
        //    await _groupStore.CreateAsync(group);
        //    return IdentityResult.Success;
        //}

        //public async Task<IdentityResult> SetGroupRolesAsync(string groupId, params string[] roleNames)
        //{
        //    // Clear all the roles associated with this group:
        //    var thisGroup = await this.FindByIdAsync(groupId);
        //    thisGroup.GroupModules.Clear();
        //    await _db.SaveChangesAsync();

        //    // Add the new roles passed in:
        //    var newRoles = _roleManager.Roles.Where(r => roleNames.Any(n => n == r.Name));
        //    foreach (var role in newRoles)
        //    {
        //        thisGroup.GroupModules.Add(new RoleGroupModule { RoleGroupId = groupId, ModuleId = role.Id });
        //    }
        //    await _db.SaveChangesAsync();

        //    // Reset the roles for all affected users:
        //    foreach (var groupUser in thisGroup.UserGroup)
        //    {
        //        await this.RefreshUserGroupRolesAsync(groupUser.UserId);
        //    }
        //    return IdentityResult.Success;
        //}

        //public IdentityResult RefreshUserGroupRoles(string userId)
        //{
        //    var user = _userManager.FindById(userId);
        //    if (user == null)
        //    {
        //        throw new ArgumentNullException("User");
        //    }
        //    // Remove user from previous roles:
        //    var oldUserRoles = _userManager.GetRoles(userId);
        //    if (oldUserRoles.Count > 0)
        //    {
        //        _userManager.RemoveFromRoles(userId, oldUserRoles.ToArray());
        //    }

        //    // Find teh roles this user is entitled to from group membership:
        //    var newGroupRoles = this.GetUserGroupRoles(userId);

        //    // Get the damn role names:
        //    var allRoles = _roleManager.Roles.ToList();
        //    var addTheseRoles = allRoles.Where(r => newGroupRoles.Any(gr => gr.RoleGroupId == r.Id));
        //    var roleNames = addTheseRoles.Select(n => n.Name).ToArray();

        //    // Add the user to the proper roles
        //    _userManager.AddToRoles(userId, roleNames);

        //    return IdentityResult.Success;
        //}
        //public async Task<IdentityResult> RefreshUserGroupRolesAsync(string userId)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        throw new ArgumentNullException("User");
        //    }
        //    // Remove user from previous roles:
        //    var oldUserRoles = await _userManager.GetRolesAsync(userId);
        //    if (oldUserRoles.Count > 0)
        //    {
        //        await _userManager.RemoveFromRolesAsync(userId, oldUserRoles.ToArray());
        //    }

        //    // Find the roles this user is entitled to from group membership:
        //    var newGroupRoles = await this.GetUserGroupRolesAsync(userId);

        //    // Get the damn role names:
        //    var allRoles = await _roleManager.Roles.ToListAsync();
        //    var addTheseRoles = allRoles.Where(r => newGroupRoles.Any(gr => gr.ModuleId == r.Id));
        //    var roleNames = addTheseRoles.Select(n => n.Name).ToArray();

        //    // Add the user to the proper roles
        //    await _userManager.AddToRolesAsync(userId, roleNames);

        //    return IdentityResult.Success;
        //}
        //public IEnumerable<RoleGroupModule> GetUserGroupRoles(string userId)
        //{
        //    var userGroups = this.GetUserGroups(userId);
        //    var userGroupRoles = new List<RoleGroupModule>();
        //    foreach (var group in userGroups)
        //    {
        //        userGroupRoles.AddRange(group.GroupModules.ToArray());
        //    }
        //    return userGroupRoles;
        //}

        //public async Task<IEnumerable<RoleGroupModule>> GetUserGroupRolesAsync(string userId)
        //{
        //    var userGroups = await this.GetUserGroupsAsync(userId);
        //    var userGroupRoles = new List<RoleGroupModule>();
        //    foreach (var group in userGroups)
        //    {
        //        userGroupRoles.AddRange(group.GroupModules.ToArray());
        //    }
        //    return userGroupRoles;
        //}
        //public IEnumerable<RoleGroup> GetUserGroups(string userId)
        //{
        //    var result = new List<RoleGroup>();
        //    var userGroups = (from g in this.Groups
        //                      where g.UserGroup.Any(u => u.UserId == userId)
        //                      select g).ToList();
        //    return userGroups;
        //}

        //public async Task<IEnumerable<RoleGroup>> GetUserGroupsAsync(string userId)
        //{
        //    var result = new List<RoleGroup>();
        //    var userGroups = (from g in this.Groups
        //                      where g.UserGroup.Any(u => u.UserId == userId)
        //                      select g).ToListAsync();
        //    return await userGroups;
        //}
        //public async Task<RoleGroup> FindByIdAsync(string id)
        //{
        //    return await _groupStore.FindByIdAsync(id);
        //}


        //public RoleGroup FindById(string id)
        //{
        //    return _groupStore.FindById(id);
        //}

        //public IQueryable<RoleGroup> Groups
        //{
        //    get
        //    {
        //        return _groupStore.Groups;
        //    }
        //}
    }

    [Serializable]
    public class GroupExistsException : Exception
    {
        public GroupExistsException()
        {
        }

        public GroupExistsException(string message) : base(message)
        {
        }

        public GroupExistsException(string message, Exception inner) : base(message, inner)
        {
        }

        //protected GroupExistsException(
        //    SerializationInfo info,
        //    StreamingContext context) : base(info, context)
        //{
        //}
    }

    public class GroupStoreBase
    {
        public DbContext Context
        {
            get;
            private set;
        }


        public DbSet<RoleGroup> DbEntitySet
        {
            get;
            private set;
        }


        public IQueryable<RoleGroup> EntitySet
        {
            get
            {
                return this.DbEntitySet;
            }
        }


        public GroupStoreBase(DbContext context)
        {
            this.Context = context;
            this.DbEntitySet = context.Set<RoleGroup>();
        }


        public void Create(RoleGroup entity)
        {
            this.DbEntitySet.Add(entity);
        }


        public void Delete(RoleGroup entity)
        {
            this.DbEntitySet.Remove(entity);
        }


        public virtual Task<RoleGroup> GetByIdAsync(object id)
        {
            return this.DbEntitySet.FindAsync(new object[] { id });
        }


        public virtual RoleGroup GetById(object id)
        {
            return this.DbEntitySet.Find(new object[] { id });
        }


        public virtual void Update(RoleGroup entity)
        {
            if (entity != null)
            {
                this.Context.Entry<RoleGroup>(entity).State = EntityState.Modified;
            }
        }
    }
}