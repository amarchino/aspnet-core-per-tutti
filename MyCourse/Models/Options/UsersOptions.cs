using System.Collections.Generic;

namespace MyCourse.Models.Options
{
    public class UsersOptions
    {
        public Dictionary<string, List<string>> AssignRolesOnRegistration { get; set; }
        public string AssignAdministratorRoleOnRegistration { get; set; }
        public string NotificationEmailRecipient { get; set; }
    }
}
