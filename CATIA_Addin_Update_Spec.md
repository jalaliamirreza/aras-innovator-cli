# CATIA Add-in Update Specification

## Purpose

Update the existing CATIA add-in to handle Aras versioning and lifecycle.

---

## Current State

| Feature | Status |
|---------|--------|
| Check-in | ✅ Works |
| Check-out | ✅ Works |
| Locking | ✅ Works |
| Version handling | ❌ Not implemented |

---

## Aras Configuration (Already Done)

| Setting | Value |
|---------|-------|
| Lifecycle | CAD (Preliminary → In Review → Released → Superseded → Obsolete) |
| Revision | A, B, C, D... |
| File behavior | Fixed (files don't copy to new revision) |

---

## Update 1: Check-out

### Flow

1. User clicks Check-out
2. Show search dialog
3. User selects document
4. Download file
5. Lock document in Aras
6. Open in CATIA

### Display in Search Results

| Column | Source |
|--------|--------|
| Item Number | item_number |
| Name | name |
| Revision | major_rev |
| State | state |
| Locked By | locked_by_id |

### Validation

| Check | Action |
|-------|--------|
| Already locked by other user | Show error: "Locked by [username]" |
| Already locked by me | Allow (already have lock) |

---

## Update 2: Check-in

### Flow

1. User clicks Check-in
2. Get current document path from CATIA
3. Find matching document in Aras
4. Check state
5. If Preliminary → Upload file, unlock
6. If Released → Show error

### Validation

| State | Can Check-in? | Message |
|-------|---------------|---------|
| Preliminary | ✅ Yes | Upload and unlock |
| In Review | ❌ No | "Document is In Review" |
| Released | ❌ No | "Document is Released. Create new revision in Aras first." |
| Obsolete | ❌ No | "Document is Obsolete" |

### On Success

1. Upload file to Aras
2. Unlock document
3. Show confirmation with revision info

---

## Update 3: Display Info

### After Check-out, Show

```
Document: [Item Number]
Revision: [A/B/C]
State: [Preliminary/Released/etc]
Status: Locked by you
```

### After Check-in, Show

```
Document: [Item Number]  
Revision: [A/B/C]
State: [State]
Status: Unlocked
File uploaded successfully
```

---

## Update 4: New Revision Handling

### When to Create New Revision

User must create new revision in Aras web interface when:
- Document is Released
- Changes are needed

### Add-in Behavior

Add-in does NOT create new revisions automatically.

If user tries to check-in to Released document:
- Show error
- Instruct user to create new revision in Aras
- Then check-out the new revision

---

## User Workflow

### Scenario A: Working on Preliminary Document

| Step | User Action | System Response |
|------|-------------|-----------------|
| 1 | Check-out DOC-001 Rev A | Downloads file, locks in Aras |
| 2 | Modify in CATIA | - |
| 3 | Check-in | Uploads file, unlocks |
| 4 | Done | Rev A updated |

### Scenario B: Need to Modify Released Document

| Step | User Action | System Response |
|------|-------------|-----------------|
| 1 | Check-out DOC-001 Rev A (Released) | Downloads file, locks in Aras |
| 2 | Modify in CATIA | - |
| 3 | Check-in | ERROR: "Document is Released" |
| 4 | Go to Aras web | Create new revision (Rev B) |
| 5 | Check-out Rev B | Downloads file, locks |
| 6 | Check-in | Uploads file, unlocks |
| 7 | Done | Rev B updated |

---

## API Calls Needed

### Check-out

1. Search document (get id, state, locked_by)
2. Lock document
3. Download file

### Check-in

1. Get document by id
2. Check state
3. If Preliminary: Upload file
4. Unlock document

### Get Document Info

| Property | Aras Field |
|----------|------------|
| Item Number | item_number |
| Name | name |
| Revision | major_rev |
| State | state |
| Locked By | locked_by_id |

---

## Error Messages

| Situation | Message |
|-----------|---------|
| Document locked by other | "Document is locked by [username]" |
| Check-in to Released | "Cannot check-in. Document is Released. Create new revision in Aras first." |
| Check-in to In Review | "Cannot check-in. Document is In Review." |
| Not locked by you | "You don't have this document checked out." |
| File not found | "File not found in CATIA." |

---

## Summary of Changes

| Component | Change |
|-----------|--------|
| Search dialog | Add Revision, State, Locked By columns |
| Check-out | Show revision and state info |
| Check-in | Check state before upload |
| Error handling | State-based error messages |
| Info display | Show revision and state |

---

## Not in Scope

| Feature | Reason |
|---------|--------|
| Create new revision from add-in | User does in Aras web |
| Promote lifecycle | User does in Aras web |
| BOM Sync | Future phase |
| Property Sync | Future phase |

---

## Document Info

| Field | Value |
|-------|-------|
| Version | 1.0 |
| Date | December 2024 |
| Status | Ready for Development |

---

**END OF SPECIFICATION**
