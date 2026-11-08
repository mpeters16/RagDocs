# RagDocs.Web

The browser front end for RagDocs. It lets you upload documents (PDF, DOCX, TXT, Markdown), watch them get processed in the background, and delete them when you no longer need them.

Built with React 19, TypeScript, Vite, and React Router.

## Features

- **Upload** PDF, DOCX, TXT, and Markdown files from the Documents page.
- **Track processing status** for each document: `Pending`, `Processing`, `Ready`, or `Failed`. A failed document shows the reason it failed.
- **See chunk counts** for documents that have finished processing.
- **Delete documents** after a confirmation prompt. Documents that are still processing can't be deleted.
- **Duplicate detection.** If you upload a file that was already ingested, the app tells you instead of adding it twice.

## Prerequisites

- Node.js (a current LTS release) and npm
- The RagDocs API running locally at `https://localhost:7121`. The dev server proxies `/api` to this address and accepts its development certificate.

## Getting started

```bash
cd src/RagDocs.Web
npm install
npm run dev
```

Open the URL Vite prints (by default `http://localhost:5173`).

## Scripts

| Command           | What it does                                              |
| ----------------- | --------------------------------------------------------- |
| `npm run dev`     | Starts the Vite dev server with hot module replacement.   |
| `npm run build`   | Type-checks with `tsc -b`, then builds to `dist/`.        |
| `npm run preview` | Serves the production build locally for a quick check.    |
| `npm run lint`    | Runs ESLint over the project.                             |

## Project structure

```
src/
├── main.tsx                     # App bootstrap (router + React root)
├── App.tsx                      # Top bar, navigation, and route table
├── index.css                    # Global styles
├── api/
│   ├── client.ts                # fetch wrapper and typed API methods
│   └── types.ts                 # DTOs and ProblemDetails shapes from the API
└── documents/
    ├── DocumentsPage.tsx        # Page: upload form, loading/error states, table
    ├── UploadForm.tsx           # File picker and upload submission
    ├── DocumentsTable.tsx       # Document list with status, size, and delete
    ├── StatusBadge.tsx          # Colored badge for a document status
    └── useDocuments.ts          # Hook that loads the list and exposes refresh()
```

## How it talks to the API

All requests go through `src/api/client.ts`, which prefixes paths with `/api`. In development, Vite's proxy (see `vite.config.ts`) strips that prefix and forwards the request to `https://localhost:7121`.

| Method   | Path              | Used for                          |
| -------- | ----------------- | --------------------------------- |
| `GET`    | `/api/documents`  | Listing documents                 |
| `POST`   | `/api/documents`  | Uploading a file (multipart form, field `file`) |
| `DELETE` | `/api/documents/{id}` | Deleting a document           |

Non-2xx responses are turned into an `ApiError`. When the API returns RFC 7807 `ProblemDetails`, its `detail` or `title` is shown to the user.

After an upload or delete, the page reloads the list so the table reflects the server state. Documents in `Pending` or `Processing` status are not refreshed automatically, so reload the page to see them move to `Ready` or `Failed`.

## Configuration

There is no environment configuration. The only setting is the API target in `vite.config.ts`:

```ts
target: 'https://localhost:7121'
```

Change it there if your API runs on a different address.

## Notes

- The Vite dev proxy disables certificate verification (`secure: false`) so it can accept the ASP.NET development certificate. This applies to the dev server only.
- Routing uses React Router. Currently there is one route, `/`, for the Documents page.
