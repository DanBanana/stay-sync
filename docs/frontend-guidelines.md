# Frontend UI Implementation Guide (Angular + SCSS, Flexbox, Grid, Angular Material)

Production-grade best practices for building clean, scalable, and responsive UIs using Angular, SCSS, Flexbox, Grid, and Angular Material.

---

## 0. Core Mindset (Angular Context)

- **Component-first architecture**
- Each UI piece = isolated, reusable component
- **Angular Material = UI components**
- **You = layout, spacing, structure, discipline**
- Optimize for **clarity, maintainability, and scalability**

---

## 1. HTML Structure & Semantics (Angular Templates)

- Use semantic elements inside templates (`header`, `main`, `section`, etc.)
- Avoid unnecessary wrappers (no “div soup”)
- Maintain proper heading hierarchy (`h1 → h6`)
- Accessibility:
  - `label` for inputs
  - `alt` for images
  - ARIA only when necessary

- Keep templates clean and readable

---

## 2. Angular Component Structure

- One responsibility per component
- Co-locate files:
  - `component.ts`
  - `component.html`
  - `component.scss`

- Keep components:
  - Small
  - Reusable
  - Focused

**Avoid:**

- Huge “god components”
- Mixing layout + business logic + styling excessively

---

## 3. SCSS Architecture (Angular-Aware)

- Prefer **component-scoped styles**
- Keep global styles (`styles.scss`) minimal:
  - resets
  - typography base
  - variables/tokens

### Structure

- `variables` (colors, spacing, typography, breakpoints)
- `mixins/functions`
- `base`
- `layout`
- `components`
- `utilities`

### Rules

- Avoid deep nesting (max 2–3 levels)
- Use variables instead of hardcoded values
- Keep specificity low
- Reuse via mixins/functions

---

## 4. Angular View Encapsulation (Important)

- Styles are **scoped per component by default**
- Do NOT rely on global overrides
- Prefer local styling inside component SCSS

**Avoid:**

```scss
::ng-deep .mat-button { ... }
```

Only use deep overrides as a last resort.

---

## 5. Angular Material Integration

### Use Material for:

- Buttons, forms, dialogs, cards, nav, etc.

### Do NOT:

- Fight or override internal structure heavily
- Rewrite Material components from scratch

### Instead:

- Wrap Material components cleanly
- Use Material theming system
- Extend, don’t override

---

## 6. Flexbox vs Grid

### Flexbox → 1D layouts

- Row OR column
- Alignment, spacing
- Toolbars, nav, small components

```scss
.container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}
```

### Grid → 2D layouts

- Pages, sections, card layouts

```scss
.grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
}
```

### Rule

- Don’t overengineer
- Combine both when needed

---

## 7. Layout Strategy (Critical in Angular)

- Angular Material does NOT handle layout well
- Always define layout using:
  - Flexbox
  - Grid

Example:

```html
<div class="page">
  <mat-card>...</mat-card>
  <mat-card>...</mat-card>
</div>
```

```scss
.page {
  display: grid;
  gap: 1rem;
}
```

---

## 8. Spacing System

- Use consistent scale (4px or 8px system)
- Use:
  - `margin` → external spacing
  - `padding` → internal spacing

- Prefer `gap` over margin hacks
- Avoid magic numbers

---

## 9. Typography

- Define type scale (e.g., 12, 14, 16, 20, 24, 32…)
- Limit to 1–2 font families
- Use:
  - `line-height: 1.4–1.6`
  - readable line length (60–75 chars)

- Prefer `rem/em` over `px`
- Ensure strong contrast

---

## 10. White Space & Hierarchy

- Use whitespace intentionally
- Group related elements visually
- Emphasize hierarchy via:
  - size
  - spacing
  - weight

- Avoid clutter

---

## 11. Responsiveness

- Mobile-first (`min-width`)
- Use fluid layouts:
  - `%`, `flex`, `grid`

- Use responsive units:
  - `rem`, `%`, `vw/vh`

- Standardize breakpoints (`sm`, `md`, `lg`, `xl`)
- Test on real devices

---

## 12. Scroll Behavior (Strict Rule)

- **Avoid scrollbars whenever possible**
- Layouts should **fit naturally within viewport**
- Use:
  - flexible sizing
  - fluid layouts
  - proper wrapping

**Only allow scroll when:**

- Content is inherently long (feeds, logs, tables)
- No better UX solution exists

**Avoid:**

- Nested scroll containers
- Fixed-height containers causing overflow

---

## 13. Naming Conventions (Angular-Adjusted)

### Prefer component-based naming:

```scss
// user-card.component.scss
.container {
}
.title {
}
.actions {
}
```

### Use BEM when needed:

```scss
.card {
}
.card__title {
}
.card--highlighted {
}
```

### Rules:

- Avoid visual names (`.big-text`)
- Prefer semantic names (`.card-title`)
- Be consistent

---

## 14. Performance & Maintainability

- Keep CSS modular and scoped
- Remove unused styles
- Minimize bundle size
- Avoid layout thrashing
- Use utility classes for repeatable patterns

---

## 15. Common Pitfalls to Avoid

- Deep SCSS nesting
- Overriding Angular Material aggressively
- Using `::ng-deep` excessively
- Hardcoded sizes everywhere
- Ignoring accessibility
- Inconsistent spacing/typography
- Overusing scrollbars
- Mixing layout responsibilities

---

## 16. Guiding Principles

- Clarity > cleverness
- Consistency > creativity
- Simplicity > overengineering
- Components > monoliths
- UX first, always

---

## Final Rule

All HTML, SCSS, and layout decisions must:

- Respect Angular component boundaries
- Work with Angular Material (not against it)
- Follow consistent spacing, typography, and responsiveness
- Avoid unnecessary scrollbars
- Be clean, readable, and maintainable
