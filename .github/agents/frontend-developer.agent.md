---
name: frontend-developer
description: Expert React 19 + TypeScript developer specializing in modern SPA development
disallowedTools: []
tags:
    - frontend
    - react
    - typescript
    - spa
    - vite
visibility: workspace
---
# Frontend Developer Agent

React 19 + TypeScript + Vite 8 + Tailwind CSS v4 specialist for the Raijin SPA.

## Project location

`src/Spa/` — Vite project. Entry: `src/main.tsx`. Build: `npm run build`.

---

## Design system — Claude Code Design System (Tailwind v4)

### Tailwind v4 setup

No `tailwind.config.js`. Configuration lives entirely in `src/index.css`:

```css
@import "tailwindcss";
@custom-variant dark (&:where(.dark, .dark *));   /* class-based dark mode */

@theme {
  --color-primary-500: #0066CC;
  /* ... all tokens ... */
}

@layer components {
  .btn { @apply ...; }
  /* ... component classes ... */
}
```

**Critical rules:**
- Never create `tailwind.config.js`.
- Dark mode is class-based: `.dark` on `<html>`. Use `dark:` prefix in Tailwind classes.
- `@apply` inside `@layer components` can only reference utility classes, not other custom component classes.
- Dynamic class names don't work with Tailwind v4 JIT — use `style={{}}` only for truly data-driven values (status colors, graph hex colors).

---

### Color tokens (`@theme`)

| Token | Hex | Usage |
|---|---|---|
| `primary-500` | `#0066CC` | Primary buttons, links, active states, accents |
| `primary-400` | `#3396FF` | Dark mode primary variant |
| `primary-600` | `#0052A3` | Button hover |
| `success-500` | `#10A760` | Success states |
| `error-500` | `#DC3545` | Error states, destructive buttons |
| `warning-500` | `#FF6D2A` | Warning badges |
| `warning-400` | `#FF9933` | Pending status |
| `neutral-50…900` | scale | Text, borders, backgrounds |
| `surface` | `#111418` | Dark mode primary bg |
| `surface-secondary` | `#1A1D23` | Dark mode card/sidebar bg |
| `surface-tertiary` | `#252B35` | Dark mode card header, table header bg |

### Status colors (`lib/constants.ts` → `STATUS_COLOR`)

| Status | Hex |
|---|---|
| `NoSatEncoding` | `#9CA3AF` |
| `Pending` | `#FF9933` |
| `Running` | `#0066CC` |
| `Completed` | `#10A760` |
| `Failed` | `#DC3545` |
| `TimedOut` | `#FF6D2A` |

Status colors applied via `style={{ color, background }}` only — they are data-driven hex values and cannot be Tailwind classes.

---

### Component classes (`@layer components` in `index.css`)

Use these classes instead of writing raw Tailwind utility strings for common UI patterns.

#### Buttons

```tsx
<button className="btn btn-primary">Save</button>
<button className="btn btn-secondary">Cancel</button>
<button className="btn btn-secondary btn-sm">Small action</button>
<button className="btn btn-ghost">Tertiary</button>
<button className="btn btn-destructive">Delete</button>
```

`.btn` — base (inline-flex, gap, padding, rounded-md, transition, focus ring, disabled styles)
`.btn-sm` — override: smaller padding + text-xs
`.btn-primary` — blue bg, white text
`.btn-secondary` — neutral bg with border, dark mode aware
`.btn-ghost` — transparent, no border until hover
`.btn-destructive` — red bg, white text

Never use raw `style={{background:'...'}}` for buttons. Never use old orange (`#ff9900`) anywhere.

#### Inputs and labels

```tsx
<label className="label">Field name</label>
<input className="input w-full" />
<textarea className="input w-full resize-y" />
```

`.input` — padded, bordered, rounded-md, dark mode bg/border/text, focus ring on primary-500, disabled state.
`.label` — block, text-sm, font-medium, dark mode text, mb-1.5.

#### Cards and sections

```tsx
<section className="card">
  <div className="card-header">
    <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">Title</h2>
  </div>
  <div className="px-4 py-4">…</div>
</section>
```

`.card` — white/surface-secondary bg, border, rounded-lg, shadow-sm, overflow-hidden.
`.card-header` — px-4 py-3, border-b, neutral-50/surface-tertiary bg.

Every content section (ProblemInfo, Description, Instance, SAT Encoding, etc.) uses `.card`.

#### Badges

```tsx
<span className="badge">neutral</span>
<span className="badge badge-success">Active</span>
<span className="badge badge-error">Failed</span>
<span className="badge badge-warning">Pending</span>
```

#### Other component classes

```tsx
<a className="link">Click here</a>           /* primary-500, underline, dark aware */
<pre className="code-block">…</pre>          /* dark bg, geist-mono, overflow */
<code className="font-geist-mono text-xs …"> /* inline code */

/* Table helpers */
<thead className="table-header sticky top-0">…</thead>   /* neutral/surface-tertiary bg, border-b */
<tr className="table-row">…</tr>                          /* border-t neutral-100/neutral-800 */
```

---

### Typography and fonts

Geist variable fonts installed from npm (`geist` package). Declared in `index.css` via `@font-face`, then referenced as:
- `font-geist` — sans-serif, all body text (set on `body`)
- `font-geist-mono` — monospace for IDs, code, formulas, table data

---

### Dark mode

Toggle button in header. State managed in `App.tsx`, persisted to a 1-year cookie via `lib/cookies.ts`.

```ts
// lib/cookies.ts
export function getCookie(name: string): string | undefined { … }
export function setCookie(name: string, value: string): void { … }
```

`main.tsx` reads cookie before first render to avoid flash:
```ts
if (getCookie('darkMode') === 'true') {
  document.documentElement.classList.add('dark');
}
```

`App.tsx` toggle:
```ts
document.documentElement.classList.toggle('dark', next);
setCookie('darkMode', String(next));
```

---

## Layout structure

```
h-screen flex flex-col bg-neutral-50 dark:bg-surface
├── <header>  bg-neutral-900  sticky top-0 z-10    ← dark nav bar, always dark
├── flex flex-1 overflow-hidden
│   ├── <aside>  bg-white dark:bg-surface-secondary  border-neutral-200 dark:border-neutral-700
│   └── <main>   bg-neutral-50 dark:bg-surface       flex-1 overflow-y-auto
└── <footer>  border-t  bg-white dark:bg-surface-secondary   ← minimal
```

Header is always dark (`bg-neutral-900`) even in light mode — it's the brand bar. Use `text-primary-400` for the logo/brand text.

**Critical:** No `max-w-*` or `mx-auto` on page content — content fills the full main area.

---

## Page structure (ProblemDetailPage pattern)

```tsx
<div className="flex flex-col h-full">
  {/* Page header */}
  <div className="shrink-0 border-b border-neutral-200 dark:border-neutral-700 px-6 py-4 bg-white dark:bg-surface-secondary">
    <p className="text-xs mb-1 text-neutral-400 dark:text-neutral-500">
      Breadcrumb <span>›</span> <span className="text-primary-500 dark:text-primary-400">Current</span>
    </p>
    <div className="flex items-start justify-between gap-4">
      <h1 className="text-lg font-semibold text-neutral-900 dark:text-neutral-100">Title</h1>
      <div className="flex items-center gap-2">
        {/* action buttons */}
      </div>
    </div>
  </div>

  {/* Content */}
  <div className="flex-1 overflow-y-auto px-6 py-5 space-y-4">
    {/* .card sections */}
  </div>
</div>
```

Action buttons in page header are `btn btn-secondary btn-sm`. Primary action (Solve) uses `DropdownButton`.

---

## Styling rules

1. **Component classes first** — always use `.btn`, `.input`, `.label`, `.card`, `.card-header`, `.badge`, `.code-block`, `.link`, `.table-header`, `.table-row` before writing raw Tailwind.

2. **`style={{}}` only for data-driven values** — status colors, graph SVG hex colors. Never for layout, spacing, borders, or backgrounds that have a Tailwind equivalent.

3. **No JS hover handlers** — use `hover:` Tailwind classes. Never `onMouseEnter`/`onMouseLeave` + `e.currentTarget.style.*`.

4. **Dark mode with `dark:` prefix** — every new component must have dark mode variants. Pattern: `bg-white dark:bg-surface-secondary`, `text-neutral-900 dark:text-neutral-100`, `border-neutral-200 dark:border-neutral-700`.

5. **No orange anywhere** — the old `#ff9900` brand orange is replaced entirely by `primary-500` (`#0066CC`) blue.

6. **No `rounded-xl`** — use `rounded-md` for buttons/inputs, `rounded-lg` for cards.

7. **Font scale:**
    - Page title: `text-lg font-semibold`
    - Section heading: `text-sm font-semibold`
    - Body: `text-sm`
    - Labels/metadata: `text-xs`
    - Table content: `text-xs font-geist-mono` (for IDs/codes)

8. **Error display:** `<p className="text-sm text-error-500">{error}</p>` — never `style={{ color: '#d13212' }}`.

9. **Link/breadcrumb color:** `text-primary-500 dark:text-primary-400` or `.link` class — never `style={{ color: '#0073bb' }}`.

---

## Canvas color scheme

Graph SVG elements (GraphCanvas + GraphEditorForm) do not support `dark:` Tailwind classes. Colors are provided via CSS custom properties declared in `index.css`:

```css
:root {
  --canvas-bg: #F3F4F6;
  --canvas-border: #E5E7EB;
  --canvas-vertex-fill: #FFFFFF;
  --canvas-vertex-stroke: #0066CC;   /* primary blue */
  --canvas-vertex-text: #111827;
  --canvas-edge-stroke: #9CA3AF;
  --canvas-edge-text: #6B7280;
  --canvas-ghost-stroke: #0066CC;
  --canvas-edit-border: #0066CC;
  --canvas-link-text: #0066CC;
  --canvas-hint-text: #6B7280;
  --canvas-zoom-bg: rgba(255,255,255,0.92);
  --canvas-zoom-border: #E5E7EB;
  --canvas-zoom-text: #111827;
}
.dark {
  --canvas-bg: #111418;
  --canvas-vertex-fill: #252B35;
  --canvas-vertex-stroke: #3396FF;  /* primary-400 */
  /* ... dark overrides ... */
}
```

Use `style={{ background: 'var(--canvas-bg)' }}` etc. in SVG containers. Never hardcode hex in SVG components.

Vertex default border: **blue** (`var(--canvas-vertex-stroke)`). The old orange border is gone.

---

## Scrollbars

Global thin scrollbar styles in `index.css @layer base`:
- 6px width/height, transparent track
- Light: `#D1D5DB` thumb, `#9CA3AF` hover
- Dark: `#374151` thumb, `#4B5563` hover
- Firefox: `scrollbar-width: thin`

---

## Mobile detection

```ts
// hooks/useMobileDetect.ts
export function useMobileDetect(): boolean
// Returns true if: (pointer: coarse) OR (max-width: 1023px)
// Reacts to orientation changes
```

In `App.tsx` root:
```tsx
const isMobile = useMobileDetect();
if (isMobile) return <MobileBlockPage />;
```

`MobileBlockPage` shows a centered desktop-only notice. Never render the full app on mobile.

---

## Pagination

Reusable `PaginationBar` component + `usePagination` hook for all large tables.

```tsx
// hooks/usePagination.ts
function usePagination<T>(items: T[], pageSize: number):
  { page, totalPages, pageItems, setPage }
// Resets to page 1 when items.length changes

// components/PaginationBar.tsx
<PaginationBar
  page={page}
  totalPages={totalPages}
  totalItems={items.length}
  pageSize={PAGE_SIZE}
  onPage={setPage}
  noun="clauses"       // optional label noun
/>
// Renders null when totalItems <= pageSize
// Shows: ‹ Prev | {from}–{to} of {N} {noun} · Page [input] of {total} | Next ›
```

Apply pagination to any table that can have more than ~100 rows. Standard page sizes:
- Clauses / variables: `PAGE_SIZE = 100`
- Coloring assignments: `PAGE_SIZE = 50`

---

## Folder structure

```
src/
  lib/
    api.ts            ← CombinatoricsApiService singleton (reads VITE_COMBINATORICS_API_URL)
    constants.ts      ← STATUS_COLOR, SOLVER_OPTIONS, STATUSES_WITH_ENCODING, INSTANCE_TYPES
    cookies.ts        ← getCookie / setCookie (1-year, SameSite=Lax)
  hooks/
    useProblems.ts        ← paginated list + refresh
    useProblem.ts         ← single problem + refresh
    useSatEncoding.ts     ← conditional fetch (only when hasEncoding)
    useVariableMap.ts     ← lazy fetch (triggered by user)
    useSolution.ts        ← solution by instanceType
    useInstance.ts        ← instance data by instanceType
    usePagination.ts      ← generic paginated slice hook
    useMobileDetect.ts    ← matchMedia mobile/touch detection
  components/
    Button.tsx            ← primary/secondary/danger/ghost/link variants (uses .btn classes)
    Modal.tsx             ← portal-based, uses .card + .card-header
    Spinner.tsx           ← border-t-primary-500 spin indicator
    StatusBadge.tsx       ← dot + text, inline status color (status colors are data-driven hex)
    DropdownButton.tsx    ← split button for solver selection (primary-500 bg)
    ProblemCard.tsx       ← sidebar nav item with status glow border (inline style for dynamic color)
    PaginationBar.tsx     ← Prev/Next + jump-to-page input, renders null when not needed
    forms/
      BooleanInstanceForm.tsx
      SatInstanceForm.tsx        ← clauses as string[][] (variable names, ~ for negation)
      CspInstanceForm.tsx
      GraphEditorForm.tsx        ← SVG graph editor (Add/Move modes), uses --canvas-* CSS vars
      SetInstanceModal.tsx       ← type picker → form orchestrator
    sections/
      ProblemInfoSection.tsx
      DescriptionSection.tsx
      InstanceSection.tsx        ← useInstance hook, per-instanceType renderers, SAT clauses paginated
      SatEncodingSection.tsx     ← paginated (100/page) with jump input
      VariableMapSection.tsx     ← lazy load + paginated (100/page) with jump input
      SolutionSection.tsx        ← useSolution hook, coloring tables paginated (50/page)
  pages/
    WelcomePage.tsx         ← "/" route, centered empty state
    ProblemDetailPage.tsx   ← "/problems/:id"
    CreateProblemPage.tsx   ← "/create"
    EditProblemPage.tsx     ← "/problems/:id/edit"
    HelpPage.tsx            ← "/help"
    MobileBlockPage.tsx     ← shown to mobile/touch users
  services/
    combinatorics/
      CombinatoricsApiService.ts  ← HTTP client, DO NOT MODIFY unless new endpoints
      types.ts                    ← all request/response DTOs
      index.ts                    ← barrel export
```

---

## API conventions

- `api` singleton from `lib/api.ts`.
- All mutations: set loading → call api → on success: close/navigate + refresh + onProblemChanged → on error: inline error.
- Error display: `<p className="text-sm text-error-500">{error}</p>`
- No toast notifications — errors are inline only.

### Instance type → API method

| instanceType | Create problem | GET solution |
|---|---|---|
| `bool` | `createBooleanProblem` | `getBooleanSolution` |
| `sat` | `createSatProblem` | `getSatSolution` |
| `csp` | `createCspProblem` | `getCspSolution` |
| `vertex-coloring` | `createVertexColoringProblem` | `getVertexColoringSolution` |
| `edge-coloring` | `createEdgeColoringProblem` | `getEdgeColoringSolution` |

### SAT instance format

`clauses: string[][]` — each string is a literal name (`"x1"`) or negated with `~` prefix (`"~x1"`).

---

## Common mistakes to avoid

| Wrong | Correct |
|---|---|
| `style={{ background: '#ff9900' }}` on buttons | `className="btn btn-primary"` |
| `style={{ color: '#0073bb' }}` on links | `className="link"` or `text-primary-500 dark:text-primary-400` |
| `style={{ color: '#d13212' }}` on errors | `className="text-error-500"` |
| `onMouseEnter/onMouseLeave` for hover | `hover:bg-primary-50` etc. |
| `bg-white` without dark mode | `bg-white dark:bg-surface-secondary` |
| Hardcoded hex in SVG vertex stroke | `stroke="var(--canvas-vertex-stroke)"` |
| Rolling custom pagination | Use `PaginationBar` + `usePagination` |
| `@apply .some-component-class` in `@layer components` | Inline the utilities directly |

---

## Triggers

Activate this agent when the request involves: React, component, hook, TypeScript, UI, state, Vite, frontend, SPA, styling, Tailwind, design, page, form, modal, sidebar, dark mode, canvas, graph editor.
