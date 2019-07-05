﻿using System;
using System.Text;
using System.Collections.Generic;
using DG.Some.Namespace;
using System.Linq;
using Microsoft.Xrm.Sdk;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using DG.Tools.XrmMockup;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using DG.XrmFramework.BusinessDomain.ServiceContext;

namespace DG.XrmMockupTest
{

    [TestClass]
    public class TestPermissions : UnitTestBase
    {

        [TestMethod]
        public void TestPermissionWhenThroughTeam()
        {
            var businessunit = new BusinessUnit();
            businessunit.Id = orgAdminUIService.Create(businessunit);
            // Create a user which does not have read access to Contact
            var user = crm.CreateUser(orgAdminUIService, businessunit.ToEntityReference(), SecurityRoles.Cannotreadcontact);
            // Create a service with the user
            var userService = crm.CreateOrganizationService(user.Id);
            // Create a Team that does have write access to Contact
            var createTeam1 = new Team
            {
                BusinessUnitId = businessunit.ToEntityReference()
            };
            var team1 = crm.CreateTeam(orgAdminUIService, createTeam1, SecurityRoles.Salesperson);
            var createTeam2 = new Team
            {
                BusinessUnitId = businessunit.ToEntityReference()
            };
            var team2 = crm.CreateTeam(orgAdminUIService, createTeam2, SecurityRoles.Salesperson);
            // Create a Contact with Team as owner
            var contact = new Contact
            {
                OwnerId = team1.ToEntityReference()
            };
            var contactId = orgAdminUIService.Create(contact);
            // Update Contact using the user service
            var updateContact = new Contact
            {
                Id = contactId,
                JobTitle = "CEO"
            };
            try
            {
                userService.Update(updateContact);
                Assert.Fail();
            } catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(FaultException));
            }

            // Add user to team
            crm.AddUsersToTeam(team2.ToEntityReference(), user.ToEntityReference());
            // Update contact using the user service
            userService.Update(updateContact);
            // Assert success
            contact = (Contact) orgAdminUIService.Retrieve(Contact.EntityLogicalName, contactId, new ColumnSet(true));
            Assert.AreEqual(contact.GetAttributeValue<string>("jobtitle"), "CEO");
        }
    }
}