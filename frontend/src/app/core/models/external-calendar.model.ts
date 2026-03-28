export type SyncStatus = 'Success' | 'Failed';

export interface ExternalCalendar {
  id: string;
  roomId: string;
  platform: string;
  icsUrl: string;
  lastSyncedAt: string | null;
  lastSyncStatus: SyncStatus | null;
  lastSyncErrorMessage: string | null;
  createdAt: string;
}

export interface SyncCalendarResult {
  inserted: number;
  updated: number;
}
