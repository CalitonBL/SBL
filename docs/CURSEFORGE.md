# CurseForge API usage

SBL is a client-side Minecraft launcher. This document describes how SBL uses the
CurseForge public API, for the API application review.

## What the API is used for

- Searching Minecraft mods and modpacks when the user types a query.
- Reading project metadata (name, summary, authors, links, categories).
- Reading the file list for a project the user selected.
- Obtaining the official download URL for a file the user chose to install.
- Importing a CurseForge modpack: reading its manifest (`projectID` / `fileID`)
  and fetching the corresponding official download URLs.

## What SBL does not do

- It does not re-host, mirror, proxy, or redistribute mod files.
- It does not bulk-scrape the API, crawl all projects, or build a public database.
- It does not bypass author permissions, paywalls, or download restrictions.
- It does not resell API data or require payment for CurseForge content.

All file downloads go to the official URL returned by the API
(`/v1/mods/{modId}/files/{fileId}/download-url`), so downloads are counted on
CurseForge and authors keep their normal statistics and monetization.

## Rate limits and caching

- Requests are made only in response to user actions (search, open project, install).
- Metadata is cached locally for a short time to avoid repeated requests.
- There is no server component and no shared API key pool.
- SBL honors HTTP 429 and backs off.

## Author permissions

SBL preserves project and file metadata, links back to the CurseForge project page,
and does not redistribute files. If an author opts out of third-party distribution,
SBL does not provide an alternative download source.

## Contact

Open an issue at <https://github.com/CalitonBL/SBL/issues>.
