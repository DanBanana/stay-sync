# Pre-Milestone Planning Checklist

> Ask the product owner these questions **before starting any new milestone**.
> Record answers before writing any code or creating a milestone doc.

---

## Standard Questions (ask every milestone)

### Product / Scope
1. **Brief summary** — What is this milestone about in one paragraph?
2. **User/product owner outcome** — In simple terms, what result should they expect after completion?
3. **Implementation approach** — Recommended plan with reasoning, including any lessons from previous milestones.
4. **Architectural decisions required** — What decisions must the product owner make before work begins?
5. **UI design choices required** — What visual/UX decisions must the product owner make?

### UI / UX (ask for any milestone with new UI components)
6. **Loading state** — Spinner or skeleton screen while data loads?
7. **Click/tap interaction** — What happens when the user clicks a list item, card, or data element?
8. **Label format** — What text appears on cards, rows, or bars? (e.g. name only, name + date, platform + range)
9. **Accessibility** — Is WCAG AA compliance required for this milestone, or best-effort?
10. **Mobile behaviour** — Horizontal scroll, list fallback, or collapsed view on small screens?

---

## Past Milestone Q&A Summaries

### Milestone 4 — Calendar Dashboard (2026-03-24)

| Question | Answer |
|---|---|
| Calendar style | Gantt/timeline (rooms as rows, dates as columns) |
| Color coding | By platform (Airbnb=red, Booking.com=blue, Expedia=yellow, Manual=grey) |
| Bar label | Platform name only |
| Bar click | Tooltip/popover → MatDialog with booking detail |
| Date window | 6-week rolling window (windowStart = today − 7 days) |
| Navigation | Prev/next buttons shift window by 1 week |
| Caching | Smart range cache in NgRx — only fetch new days not yet loaded |
| Mobile | Horizontal scroll; room labels sticky on left |
| Loading state | Spinner (mat-progress-spinner) |
| Accessibility | Best-effort (matTooltip, aria-label on nav buttons) |
