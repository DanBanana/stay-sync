export interface ExternalCalendar {
  id: string;
  roomId: string;
  platform: string;
  icsUrl: string;
  lastSyncedAt: string | null;
  createdAt: string;
}
