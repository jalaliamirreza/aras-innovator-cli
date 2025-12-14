# Aras Innovator PDM Setup Guide

## Overview

This guide provides step-by-step instructions for configuring Aras Innovator as a Product Data Management (PDM) system. It covers data model customization, lifecycle configuration, user permissions, and best practices for managing engineering data.

---

## Table of Contents

1. [Understanding the Default Data Model](#step-1-understand-the-default-data-model)
2. [Configure Part ItemType](#step-2-configure-part-itemtype)
3. [Configure Document ItemType](#step-3-configure-document-itemtype)
4. [Configure Lifecycles](#step-4-configure-lifecycles)
5. [Configure Revision Control](#step-5-configure-revision-control)
6. [Set Up Users and Permissions](#step-6-set-up-users-and-permissions)
7. [Configure File Vault](#step-7-configure-file-vault)
8. [Configure BOM Structure](#step-8-configure-bom-structure)
9. [Configure Search and Reports](#step-9-configure-search-and-reports)
10. [Test Your Configuration](#step-10-test-your-configuration)
11. [Checklist Summary](#checklist-summary)

---

## Step 1: Understand the Default Data Model

### 1.1 Explore Existing ItemTypes

Login to Aras and navigate through these key areas:

| Menu Path | ItemType | Purpose |
|-----------|----------|---------|
| Design > Parts | Part | Core product items |
| Design > Documents | Document | Drawings, specs, files |
| Design > CAD Documents | CAD | CAD-specific documents |
| Change Management > ECRs | Express ECR | Engineering Change Requests |
| Change Management > ECOs | Express ECO | Engineering Change Orders |
| Administration > ItemTypes | ItemType | Data model definition |

### 1.2 Key Relationships to Understand

```
Part
 ├── Part BOM (child parts)
 ├── Part Document (linked documents)
 ├── Part CAD (linked CAD files)
 └── Affected Item (ECO relationship)

Document
 ├── Document File (actual files in vault)
 └── Document CAD (CAD relationships)
```

### 1.3 Practice Exercise

1. Go to **Design > Parts**
2. Click **Create New Part**
3. Fill in: Item Number, Name, Description
4. Save and explore the form tabs

---

## Step 2: Configure Part ItemType

### 2.1 Add Custom Properties

Your organization likely needs additional Part attributes. Here's how to add them:

1. Go to **Administration > ItemTypes**
2. Search for "Part" and open it
3. Go to **Properties** tab
4. Click **Create New Relationship**

#### Recommended Custom Properties for Part

| Property Name | Data Type | Purpose |
|---------------|-----------|---------|
| weight | Decimal | Part weight |
| material | String or List | Material type |
| supplier | String or Item | Vendor info |
| cost | Decimal | Estimated cost |
| lead_time | Integer | Days to procure |
| make_buy | List | Make or Buy decision |
| unit_of_measure | List | EA, KG, M, etc. |

### 2.2 Create a List for Make/Buy

1. Go to **Administration > Lists**
2. Click **Create New List**
3. Name: "Make Buy Options"
4. Go to **Values** tab → Add values:
   - Make
   - Buy
   - TBD
5. Save

### 2.3 Add Property Using the List

1. Back to Part ItemType > Properties
2. Create new property:
   - **Name**: `make_buy`
   - **Label**: "Make/Buy"
   - **Data Type**: List
   - **Data Source**: Select "Make Buy Options"
3. Save

### 2.4 Update the Part Form

After adding properties, they need to appear on the form:

1. In Part ItemType, go to **Views** tab
2. Open the default Form
3. Use Form Editor to drag new fields onto the form
4. Arrange in logical groups
5. Save

---

## Step 3: Configure Document ItemType

### 3.1 Add Custom Properties

| Property Name | Data Type | Purpose |
|---------------|-----------|---------|
| doc_type | List | Drawing, Spec, Manual, etc. |
| revision_note | Text | Change description |
| approved_by | Item (User) | Approver reference |
| approval_date | Date | When approved |

### 3.2 Create Document Type List

1. Go to **Administration > Lists** > Create New
2. Name: "Document Types"
3. Add Values:
   - Drawing
   - Specification
   - Manual
   - Report
   - 3D Model
   - Other
4. Save

### 3.3 Update the Document Form

1. Open Document ItemType
2. Go to **Views** tab
3. Edit the default Form
4. Add new fields to the form layout
5. Save

---

## Step 4: Configure Lifecycles

Lifecycles control the state/status of items and what actions are allowed.

### 4.1 Review Default Lifecycle

1. Go to **Administration > Life Cycle Maps**
2. Open "Default" or "Part" lifecycle
3. Review the states and transitions

### 4.2 Create Custom Part Lifecycle

1. Click **Create New Life Cycle Map**
2. Name: "Part Lifecycle"

#### Recommended State Flow

```
┌──────────┐     ┌───────────┐     ┌──────────┐     ┌──────────┐
│  Draft   │────►│ In Review │────►│ Released │────►│ Obsolete │
└──────────┘     └───────────┘     └──────────┘     └──────────┘
      │                │                                  ▲
      │                │                                  │
      └────────────────┴──────────────────────────────────┘
                    (Reject/Cancel)
```

#### State Configuration

| State | Can Edit? | Can Delete? | Released? |
|-------|-----------|-------------|-----------|
| Draft | Yes | Yes | No |
| In Review | No | No | No |
| Released | No | No | Yes |
| Obsolete | No | No | No |

### 4.3 Define State Transitions

| From State | To State | Transition Name |
|------------|----------|-----------------|
| Draft | In Review | Submit for Review |
| In Review | Released | Approve |
| In Review | Draft | Reject |
| Released | Obsolete | Obsolete |

### 4.4 Assign Lifecycle to Part

1. Open Part ItemType
2. Set **Default Life Cycle** to your new lifecycle
3. Save

### 4.5 Create Document Lifecycle

Repeat the process for Documents with appropriate states:

```
┌──────────┐     ┌───────────┐     ┌──────────┐
│  Draft   │────►│ In Review │────►│ Released │
└──────────┘     └───────────┘     └──────────┘
```

---

## Step 5: Configure Revision Control

### 5.1 Understand Revision Scheme

Aras uses Generation (major) and Revision (minor):

- **Generation**: A, B, C... (or 1, 2, 3...)
- **Revision**: 01, 02, 03...

Example: A-01, A-02, B-01...

### 5.2 Configure Revision Sequence

1. Go to **Administration > Revision**
2. Review or create sequences
3. Common schemes:
   - **Letters**: A, B, C, D...
   - **Numbers**: 1, 2, 3, 4...
   - **Skip letters**: A, B, C (skip I, O, Q to avoid confusion)

### 5.3 Set Revision Rules

For Part ItemType:

1. Open Part ItemType
2. Configure these properties:
   - **Is Versionable**: Yes
   - **Revision Sequence**: Select your sequence
   - **Revision Naming Rule**: Define pattern

### 5.4 Revision Best Practices

| Scenario | Revision Action |
|----------|-----------------|
| Minor change (typo, cosmetic) | Increment minor revision (A-01 → A-02) |
| Major change (form/fit/function) | Increment generation (A → B) |
| New revision from Released | Creates new generation |

---

## Step 6: Set Up Users and Permissions

### 6.1 Create Identities (Roles)

1. Go to **Administration > Identities**
2. Create role-based identities:

| Identity Name | Purpose |
|---------------|---------|
| Engineers | Design engineers |
| Managers | Engineering managers |
| Quality | Quality team |
| Purchasing | Procurement team |
| Viewers | Read-only access |

#### How to Create an Identity

1. Click **Create New Identity**
2. Enter Name and Description
3. Set **Is Alias**: No (for group identities)
4. Save

### 6.2 Create Users

1. Go to **Administration > Users**
2. Click **Create New User**
3. Fill in required fields:
   - Login Name
   - First Name / Last Name
   - Email
   - Password
4. Save

### 6.3 Assign Users to Identities

1. Open the Identity (e.g., "Engineers")
2. Go to **Members** tab
3. Add users as members
4. Save

### 6.4 Configure Permissions

1. Go to **Administration > Permissions**
2. Create permission sets for each lifecycle state

#### Example: Part Permissions by State

| Identity | Draft | In Review | Released |
|----------|-------|-----------|----------|
| Engineers | Full (Get, Update, Delete) | Read (Get) | Read (Get) |
| Managers | Full | Full | Read |
| Quality | Read | Read | Read |
| Viewers | Read | Read | Read |

#### How to Create a Permission Set

1. Click **Create New Permission**
2. Name it (e.g., "Part Draft Permissions")
3. Go to **Access** tab
4. Add identities and set their access level:
   - **Get**: Can view
   - **Update**: Can edit
   - **Delete**: Can delete
   - **Discover**: Can see in searches

### 6.5 Apply Permissions to Lifecycle

1. Open Part Lifecycle Map
2. Click on each state
3. In state properties, assign the appropriate Permission set
4. Save

---

## Step 7: Configure File Vault

### 7.1 Verify Vault Setup

1. Go to **Administration > Vaults**
2. Confirm vault path and accessibility
3. Default location: `C:\Program Files (x86)\Aras\Innovator\Vault`

### 7.2 Supported File Types for CATIA Integration

Ensure these file types are properly handled:

| Extension | Description |
|-----------|-------------|
| .CATPart | CATIA Part file |
| .CATProduct | CATIA Assembly file |
| .CATDrawing | CATIA Drawing file |
| .cgr | CATIA Graphics file |
| .pdf | PDF exports |
| .stp / .step | STEP exchange format |
| .igs / .iges | IGES exchange format |

### 7.3 Test File Upload

1. Create a test Document
2. Go to **Files** tab
3. Click **Add File**
4. Upload a sample file
5. Verify it appears and can be downloaded

### 7.4 Configure File Type Icons (Optional)

1. Go to **Administration > File Types**
2. Add custom file types for CAD files
3. Assign icons for easy identification

---

## Step 8: Configure BOM Structure

### 8.1 Review Part BOM Relationship

1. Open Part ItemType
2. Go to **Relationship Types** tab
3. Find "Part BOM" relationship
4. Review existing properties:
   - Quantity
   - Reference Designator
   - Find Number

### 8.2 Add Custom BOM Properties (if needed)

| Property | Data Type | Purpose |
|----------|-----------|---------|
| notes | String | Assembly notes |
| item_seq | Integer | Sequence on drawing |
| alternate_part | Item (Part) | Alternate part reference |
| is_spare | Boolean | Spare part indicator |

### 8.3 Configure BOM View

1. Open Part ItemType
2. Go to **Relationship Types** > Part BOM
3. Edit the Grid view to show relevant columns
4. Save

### 8.4 BOM Best Practices

| Practice | Description |
|----------|-------------|
| Use consistent UOM | Ensure all quantities use same unit of measure |
| Reference Designators | Use for electronics/electrical assemblies |
| Find Numbers | Use for balloon numbers on drawings |
| Where Used | Use built-in where-used to track part usage |

---

## Step 9: Configure Search and Reports

### 9.1 Create Saved Searches

1. Go to **Design > Parts**
2. Click **Search** icon
3. Set search criteria
4. Click **Save Search**
5. Give it a name

### 9.2 Recommended Saved Searches

| Search Name | Criteria |
|-------------|----------|
| My Draft Parts | created_by_id = current user AND state = Draft |
| Released Parts | state = Released |
| Parts Pending Review | state = In Review |
| Recent Changes | modified_on > [last 7 days] |
| My Checked Out Items | locked_by_id = current user |
| Parts Without Documents | Part Document count = 0 |

### 9.3 Configure Default Search

1. Go to **Administration > ItemTypes**
2. Open Part ItemType
3. Set **Default Search** to your preferred saved search
4. Save

### 9.4 Create Reports (Optional)

1. Go to **Administration > Reports**
2. Create custom reports using:
   - XSLT Reports
   - Query Definitions
   - Custom Methods

---

## Step 10: Test Your Configuration

### 10.1 Create Test Data

Create a simple assembly structure to test:

```
Assembly: ASSY-001 (CATProduct)
├── Part: PART-001 (CATPart) - qty: 1
├── Part: PART-002 (CATPart) - qty: 2
└── Part: PART-003 (CATPart) - qty: 4
```

### 10.2 Test Checklist

#### Part Creation Test

- [ ] Create new Part with all custom properties
- [ ] Verify custom fields appear on form
- [ ] Verify default values work correctly

#### File Management Test

- [ ] Create Document
- [ ] Upload file to Document
- [ ] Download file from Document
- [ ] Verify file integrity

#### BOM Test

- [ ] Create parent Part (Assembly)
- [ ] Add child Parts to BOM
- [ ] Set quantities
- [ ] Verify BOM display

#### Lifecycle Test

- [ ] Create Part in Draft state
- [ ] Verify user can edit in Draft
- [ ] Promote to "In Review"
- [ ] Verify editing is locked
- [ ] Promote to "Released"
- [ ] Verify item is fully locked

#### Revision Test

- [ ] With Released Part, create new revision
- [ ] Verify revision number increments
- [ ] Verify old revision remains unchanged
- [ ] Verify relationships copy correctly

#### Permission Test

- [ ] Login as Engineer user
- [ ] Verify can create/edit Draft parts
- [ ] Verify cannot edit Released parts
- [ ] Login as Viewer user
- [ ] Verify read-only access

### 10.3 Document Test Results

| Test Case | Expected Result | Actual Result | Pass/Fail |
|-----------|-----------------|---------------|-----------|
| Create Part | Part created successfully | | |
| Add custom properties | Properties saved | | |
| Upload file | File in vault | | |
| Create BOM | Children linked | | |
| Promote lifecycle | State changes | | |
| Create revision | New rev created | | |
| Permission check | Access controlled | | |

---

## Checklist Summary

### Data Model Configuration

- [ ] Explore default data model
- [ ] Add custom Part properties
- [ ] Add custom Document properties
- [ ] Create Lists (Make/Buy, Doc Types, etc.)
- [ ] Update Part Form with new fields
- [ ] Update Document Form with new fields

### Lifecycle Configuration

- [ ] Create Part Lifecycle
- [ ] Create Document Lifecycle
- [ ] Define state transitions
- [ ] Assign lifecycles to ItemTypes

### Revision Configuration

- [ ] Configure Revision sequence
- [ ] Set revision rules on ItemTypes
- [ ] Test revision creation

### Security Configuration

- [ ] Create Identities/Roles
- [ ] Create Users
- [ ] Assign Users to Identities
- [ ] Create Permission sets
- [ ] Apply Permissions to Lifecycle states

### File Management

- [ ] Verify Vault configuration
- [ ] Test File upload/download
- [ ] Configure file type icons (optional)

### BOM Configuration

- [ ] Review Part BOM relationship
- [ ] Add custom BOM properties (if needed)
- [ ] Configure BOM grid view

### Search and Reports

- [ ] Create Saved Searches
- [ ] Configure default searches
- [ ] Create Reports (optional)

### Testing

- [ ] Test with sample data
- [ ] Document test results
- [ ] Fix any issues found

---

## Next Steps

Once you complete this PDM setup, you can proceed to:

1. **Configure Workflows** - Automated approval routing
2. **Set Up Numbering Sequences** - Auto-generate part numbers
3. **Configure Email Notifications** - Alert users of tasks
4. **Develop CATIA Integration** - Connect CAD system
5. **Develop SAP Integration** - Connect ERP system

---

## Appendix A: Common Lists to Create

| List Name | Values |
|-----------|--------|
| Make Buy Options | Make, Buy, TBD |
| Document Types | Drawing, Specification, Manual, Report, 3D Model, Other |
| Unit of Measure | EA, KG, G, M, MM, CM, L, ML |
| Material Types | Steel, Aluminum, Plastic, Rubber, Copper, Other |
| Part Classification | Mechanical, Electrical, Electronic, Software, Assembly |

---

## Appendix B: Recommended Identities

| Identity | Description | Typical Permissions |
|----------|-------------|---------------------|
| Administrators | System administrators | Full access to all |
| Engineers | Design engineers | Create/Edit Draft, Read Released |
| Senior Engineers | Lead engineers | Create/Edit/Approve |
| Managers | Engineering managers | Full access, Approve |
| Quality | Quality assurance | Read access, Quality approvals |
| Purchasing | Procurement team | Read Parts, Access supplier info |
| Manufacturing | Production team | Read Released, BOM access |
| Viewers | General read access | Read only |

---

## Appendix C: Troubleshooting

### Common Issues and Solutions

| Issue | Possible Cause | Solution |
|-------|----------------|----------|
| Cannot see new property on form | Form not updated | Edit form and add field |
| Lifecycle transition fails | Missing permission | Check user identity and permissions |
| File upload fails | Vault path issue | Verify vault configuration |
| Cannot create revision | Item not released | Release item first |
| Search returns no results | Wrong criteria | Check search filters |

---

## Document Information

| Field | Value |
|-------|-------|
| Document Title | Aras Innovator PDM Setup Guide |
| Version | 1.0 |
| Created Date | December 2024 |
| Author | Implementation Team |
| Status | Draft |

---

*End of Document*
