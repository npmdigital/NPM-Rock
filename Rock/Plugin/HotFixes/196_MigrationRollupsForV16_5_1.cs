﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using Rock.SystemGuid;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 196, "1.16.4" )]
    public class MigrationRollupsForV16_5_1: Migration
    {
        private const string PublicProfileEditBlockTypeGuid = "841D1670-8BFD-4913-8409-FB47EB7A2AB9";
        private const string GenderBlockTypeAttributeGuid = "DD636ABE-3E5B-442F-9548-9F85DF768FFF";
        private const string GenderAttributeValueGuid = "6FE7E960-0DC2-4089-B346-1CD047EBE6F3";
        private const string MyAccountPageGuid = "C0854F84-2E8B-479C-A3FB-6B47BE89B795";

        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            ConsolidateGenderSettingsOnPublicProfileEditBlockUp();
            AddVolunteerGenerosityAnalysisFeature();
            DropHistoryGuidIndexUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            ConsolidateGenderSettingsOnPublicProfileEditBlockDown();
        }

        /// <summary>
        /// KA: MigrationToConsolidateGenderSettingsOnPublicProfileEditBlock
        /// </summary>
        public void ConsolidateGenderSettingsOnPublicProfileEditBlockUp()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( PublicProfileEditBlockTypeGuid, Rock.SystemGuid.FieldType.SINGLE_SELECT, "Gender", "Gender", "Gender", "How should Gender be displayed?", 26, "Required", GenderBlockTypeAttributeGuid );

            string qry = $@"
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE Guid = '{PublicProfileEditBlockTypeGuid}')
DECLARE @BlockId INT = (SELECT [Id] FROM [Block] WHERE BlockTypeId = @BlockTypeId AND PageId = (SELECT Id FROM Page WHERE Guid = '{MyAccountPageGuid}'))
DECLARE @GenderAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [KEY] = 'Gender' AND [EntityTypeQualifierValue] = @BlockTypeId)
DECLARE @RequireGenderAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [KEY] = 'RequireGender' AND [EntityTypeQualifierValue] = @BlockTypeId)
DECLARE @ShowGenderAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [KEY] = 'ShowGender' AND [EntityTypeQualifierValue] = @BlockTypeId)
DECLARE @RequireGender VARCHAR(50) = (SELECT [Value] FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @RequireGenderAttributeId)
DECLARE @ShowGender VARCHAR(50) = (SELECT [Value] FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @ShowGenderAttributeId)
DECLARE @TheValue VARCHAR(50) = CASE
	WHEN @RequireGender = 'True' AND @ShowGender = 'True' THEN 'Required'
	WHEN @RequireGender = 'True' AND @ShowGender = 'False' THEN 'Required'
	WHEN @RequireGender = 'False' AND @ShowGender = 'False' THEN 'Hide'
	WHEN @RequireGender = 'False' AND @ShowGender = 'True' THEN 'Optional'
	ELSE 'Required'
END

IF EXISTS (SELECT 1 FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @GenderAttributeId)  
BEGIN  
	UPDATE [AttributeValue]   
	SET [Value] = @TheValue,  
	[Guid] = '{GenderAttributeValueGuid}'
	WHERE [EntityId] = @BlockId AND [AttributeId] = @GenderAttributeId;  
END  
ELSE  
BEGIN  
	INSERT INTO [AttributeValue] (
	    [IsSystem],
	    [AttributeId],
	    [EntityId],
	    [Value],
	    [Guid])
	VALUES(
	    1,
	    @GenderAttributeId,
	    @BlockId,
	    @TheValue,
	    '{GenderAttributeValueGuid}') 
END
";
            Sql( qry );
        }

        /// <summary>
        /// KA: MigrationToConsolidateGenderSettingsOnPublicProfileEditBlock
        /// </summary>
        public void ConsolidateGenderSettingsOnPublicProfileEditBlockDown()
        {
            RockMigrationHelper.DeleteBlockAttribute( GenderBlockTypeAttributeGuid );
        }

        /// <summary>
        /// JDR: Add Volunteer Generosity Analysis page, block, and persisted dataset
        /// JDR: Add the Volunteer Generosity Analysis block out of the box
        /// Creating the page, block, and dataset with a set schedule. 
        /// </summary>
        private void AddVolunteerGenerosityAnalysisFeature()
        {
            // Register the EntityType for the Volunteer Generosity Analysis Block
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.VolunteerGenerosityAnalysis", "4C55BFE1-7E97-4CFB-BCB7-2015AA25D9B9", false, true );

            // Add or Update the Block Type for your Obsidian block
            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Volunteer Generosity Analysis",
                "Displays an analysis of volunteer generosity based on a persisted dataset.",
                "Rock.Blocks.Reporting.VolunteerGenerosityAnalysis",
                "Reporting",
                "586A26F1-8A9C-4AB4-B788-9B44895B9D40"
            );

            // Create the "Reports" page under the "Finance" tab
            RockMigrationHelper.AddPage( true, "7BEB7569-C485-40A0-A609-B0678F6F7240", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reports", "", "8D5917F1-4E0E-4F18-8815-62EFBF808995", "" );

            // Move the Transaction Fee Report under the newly created "Reports" page
            RockMigrationHelper.MovePage( "A3E321E9-2FBB-4BB9-8AEE-E810B7CC5914", "8D5917F1-4E0E-4F18-8815-62EFBF808995" );

            // Create the "Volunteer Generosity" page under the "Reports" page
            RockMigrationHelper.AddPage( true, "8D5917F1-4E0E-4F18-8815-62EFBF808995", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Volunteer Generosity", "", "16DD0891-E3D4-4FF3-9857-0869A6CCBA39", "" );

            // Parameters for the schedule and persisted dataset
            string scheduleName = "Volunteer Generosity Schedule";
            string scheduleDescription = "Schedule used to run the persisted dataset job for the Volunteer Generosity block.";
            string iCalendarContent = "BEGIN:VCALENDAR\r\nPRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN\r\nVERSION:2.0\r\nBEGIN:VEVENT\r\nDTEND:20240304T060100\r\nDTSTAMP:20240305T095232\r\nDTSTART:20240304T060000\r\nRRULE:FREQ=WEEKLY;BYDAY=MO\r\nSEQUENCE:0\r\nUID:577ed439-4e7e-4445-a5a3-fcf52de2174c\r\nEND:VEVENT\r\nEND:VCALENDAR";
            DateTime effectiveStartDate = DateTime.Parse( "2024-03-04" );
            bool scheduleIsActive = true;
            string scheduleGuid = "ACE62853-0A10-4523-8BA2-CF7597F1D190";
            string datasetAccessKey = "VolunteerGenerosity";
            string datasetName = "Volunteer Generosity";
            string datasetDescription = "An in-depth dataset focusing on volunteer engagement and giving behaviors, designed to support strategic decision-making for Church Leaders. This dataset is used by the Volunteer Generosity Analysis block and undergoes regular updates for optimization and improved insights during future Rock updates; editing is not advised.";
            bool datasetAllowManualRefresh = true;
            int resultFormat = 0;
            string datasetBuildScript = @"{% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}

{% sql %}
DECLARE @NumberOfDays INT = 90; 
DECLARE @NumberOfMonths INT = 13; 
DECLARE @ServingAreaDefinedValueGuid UNIQUEIDENTIFIER = '36a554ce-7815-41b9-a435-93f3d52a2828';

DECLARE @StartDateKey int = (SELECT TOP 1 [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = CAST(DATEADD(d, -@NumberOfDays, GETDATE()) AS date));
DECLARE @CurrentMonth date = DATEADD(m, DATEDIFF(m, 0, GETDATE()), 0);
DECLARE @StartingDateKeyForGiving int = (SELECT [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = DATEADD(m, -@NumberOfMonths, @CurrentMonth));

WITH AttendanceData AS (
    SELECT
        p.[Id] AS [PersonId],
        p.[LastName],
        p.[NickName],
        p.[PhotoId],
        p.[GivingId],
        g.[Id] AS [GroupId],
        g.[Name] AS [GroupName],
        c.[Id] AS [CampusId],
        c.[ShortCode],
        MAX(ao.[OccurrenceDate]) AS [LastAttendanceDate],
        gm.[GroupRoleId],
        gr.[Name] AS [GroupRoleName]
    FROM
        [Person] p
        INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
        INNER JOIN [Attendance] a ON [PersonAliasId] = pa.[Id]
        INNER JOIN [AttendanceOccurrence] ao ON ao.[Id] = a.[OccurrenceId]
        INNER JOIN [Group] g ON g.[Id] = ao.[GroupId]
        LEFT JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id] AND gm.[GroupId] = g.[Id]
        LEFT JOIN [GroupTypeRole] gr ON gr.[Id] = gm.[GroupRoleId]
        INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
        INNER JOIN [DefinedValue] dvp ON dvp.[Id] = gt.[GroupTypePurposeValueId] AND dvp.[Guid] = @ServingAreaDefinedValueGuid
        INNER JOIN [Campus] c ON c.[Id] = a.[CampusId]
    WHERE
        ao.[OccurrenceDateKey] >= @StartDateKey
        AND a.[DidAttend] = 1
    GROUP BY p.[Id], p.[LastName], p.[NickName], p.[PhotoId], p.[GivingId], g.[Id], g.[Name], g.[Guid], c.[Id], c.[Guid], c.[ShortCode], gm.[GroupRoleId], gr.[Name]
),
GivingData AS (
    SELECT
        p.[GivingId],
        asd.[CalendarMonthNameAbbreviated],
        asd.[CalendarYear],
        asd.[CalendarMonth]
    FROM
        [Person] p
        INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
        INNER JOIN [FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
        INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
        INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.[AccountId]
        INNER JOIN [AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
    WHERE
        fa.[IsTaxDeductible] = 1
        AND ft.[TransactionDateKey] >= @StartingDateKeyForGiving
    GROUP BY p.[GivingId], asd.[CalendarMonthNameAbbreviated], asd.[CalendarYear], asd.[CalendarMonth]
)

SELECT
    AD.[PersonId],
    AD.[LastName],
    AD.[NickName],
    AD.[PhotoId],
    AD.[GivingId],
    AD.[GroupId],
    AD.[GroupName],
    AD.[CampusId],
    AD.[ShortCode],
    AD.[LastAttendanceDate],
    AD.[GroupRoleId],
    AD.[GroupRoleName],
    GD.[CalendarMonthNameAbbreviated],
    GD.[CalendarYear],
    GD.[CalendarMonth]
FROM
    AttendanceData AD
    LEFT JOIN GivingData GD ON AD.[GivingId] = GD.[GivingId]
{% endsql %}

{% assign uniquePeople = results | Map: 'PersonId' | Uniq %}
{% assign uniqueGroups = results | Map: 'GroupId' | Uniq %}
{% assign uniqueGivingIds = results | Map: 'GivingId' | Uniq %}
{
    ""PeopleData"": [
        {% for personId in uniquePeople %}
            {% assign personResults = results | Where: ""PersonId"", personId %}
            {% assign firstPersonResult = personResults | First %}
            {% assign groupIdsForPerson = personResults | Map: 'GroupId' | Uniq %}
            {
                ""PersonId"": ""{{ personId }}"",
                ""LastName"": ""{{ firstPersonResult.LastName | Escape }}"",
                ""NickName"": ""{{ firstPersonResult.NickName | Escape }}"",
                ""PhotoUrl"": ""{% if firstPersonResult.PhotoId %}{{ publicApplicationRoot }}GetAvatar.ashx?PhotoId={{ firstPersonResult.PhotoId }}{% else %}{{ publicApplicationRoot }}GetAvatar.ashx?AgeClassification=Adult&Gender=Male&RecordTypeId=1&Text={{ firstPersonResult.NickName | Slice: 0, 1 }}{{ firstPersonResult.LastName | Slice: 0, 1 }}{% endif %}"",
                ""GivingId"": ""{{ firstPersonResult.GivingId }}"",
                ""GroupId"": [
                    {% for groupId in groupIdsForPerson %}
                        ""{{ groupId }}"" {% unless forloop.last %},{% endunless %}
                    {% endfor %}
                ]
            }{% unless forloop.last %},{% endunless %}
        {% endfor %}
    ],
    ""GivingData"": [
        {
            {% for givingId in uniqueGivingIds %}
                {% assign givingResults = results | Where: ""GivingId"", givingId %}
                ""{{ givingId }}"": [
                    {% for result in givingResults %}
                        {
                            ""GroupId"": ""{{ result.GroupId }}"",
                            ""MonthNameAbbreviated"": ""{{ result.CalendarMonthNameAbbreviated }}"",
                            ""Year"": ""{{ result.CalendarYear }}"",
                            ""Month"": ""{{ result.CalendarMonth }}""
                        }{% unless forloop.last %},{% endunless %}
                    {% endfor %}
                ]{% unless forloop.last %},{% endunless %}
            {% endfor %}
        }
    ],
    ""GroupData"": [
        {
            {% for groupId in uniqueGroups %}
                ""{{ groupId }}"": {
                    {% assign groupResults = results | Where: ""GroupId"", groupId %}
                    {% assign firstGroupResult = groupResults | First %}
                    ""GroupName"": ""{{ firstGroupResult.GroupName | Escape }}"",
                    ""CampusId"": ""{{ firstGroupResult.CampusId }}"",
                    ""CampusShortCode"": ""{{ firstGroupResult.ShortCode | Escape }}""
                }{% unless forloop.last %},{% endunless %}
            {% endfor %}
        }
    ]
}";

            int datasetBuildScriptType = 0;
            bool datasetIsSystem = true;
            bool datasetIsActive = true;
            string datasetEnabledLavaCommands = "All";
            string datasetGuid = "10539E72-B5D3-48E2-B9C6-DB43AFDAD55F";

            // Create the schedule and persisted dataset
            RockMigrationHelper.AddPersistedDatasetWithSchedule(
                scheduleGuid,
                scheduleName,
                scheduleDescription,
                iCalendarContent,
                effectiveStartDate,
                scheduleIsActive,
                datasetGuid,
                datasetAccessKey,
                datasetName,
                datasetDescription,
                datasetAllowManualRefresh,
                resultFormat,
                datasetBuildScript,
                datasetBuildScriptType,
                datasetIsSystem,
                datasetIsActive,
                datasetEnabledLavaCommands );

            // Add/Update the Volunteer Generosity Analysis block type
            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Volunteer Generosity Analysis",
                "Displays an analysis of volunteer generosity based on persisted dataset 'VolunteerGenerosity'.",
                "Rock.Blocks.Reporting.VolunteerGenerosityAnalysis",
                "Reporting",
                "586A26F1-8A9C-4AB4-B788-9B44895B9D40"
            );

            // Add the Volunteer Generosity Analysis block to the Volunteer Generosity page
            RockMigrationHelper.AddBlock(
                true,
                "16DD0891-E3D4-4FF3-9857-0869A6CCBA39".AsGuid(),
                null,
                "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),
                "586A26F1-8A9C-4AB4-B788-9B44895B9D40".AsGuid(),
                "Volunteer Generosity Analysis",
                "Main",
                @"",
                @"",
                0,
                "3252A755-01C1-497A-9D37-8ED91D23E061"
            );

            // Hide the Volunteer Generosity page from navigation
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) Model.DisplayInNavWhen.Never} WHERE [Guid] = '16DD0891-E3D4-4FF3-9857-0869A6CCBA39';" );
        }

        /// <summary>
        /// DL: Drop History.Guid index
        /// Drop index on [History].[Guid] column.
        /// </summary>
        private void DropHistoryGuidIndexUp()
        {
            Sql( @"
-- Drop index on [History].[Guid] column
IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_Guid' AND object_id = OBJECT_ID('History')) 
BEGIN
	DROP INDEX [IX_Guid] ON [History]
END
      " );
        }
    }
}