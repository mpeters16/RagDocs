import type { DocumentStatus } from "../api/types";


export function StatusBadge({ status }: { status: DocumentStatus  }) {
    return <span className={`badge badge-${status.toLowerCase()}`}>{status}</span>
  }