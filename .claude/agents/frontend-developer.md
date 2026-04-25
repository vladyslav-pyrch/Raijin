# Frontend Developer Agent

React 19 + TypeScript + Vite + TailwindCSS specialist for the Raijin SPA.

## Project location

`src/Spa/` — Vite project. Entry: `src/main.tsx`. Build: `npm run build`.

---

## Design system — AWS-style theme

### Color palette

All colors defined as CSS variables in `src/index.css` (`@theme` block) and used via inline `style={{}}` for dynamic values or Tailwind arbitrary values for static ones.

| Token | Hex | Usage |
|---|---|---|
| `brand-navy` | `#232f3e` | Top navigation background |
| `brand-squid` | `#16191f` | Primary text, headings |
| `brand-orange` | `#ff9900` | Primary CTA buttons, active states, accents |
| `brand-orange-dark` | `#e88b00` | Button hover, border |
| `page-bg` | `#f2f3f3` | Page/app background |
| `panel-bg` | `#ffffff` | Panel/card backgrounds |
| `border` | `#d5dbdb` | Panel borders, dividers |
| `border-light` | `#eaeded` | Table row separators |
| `text` | `#16191f` | Primary text |
| `text-secondary` | `#545b64` | Labels, secondary text |
| `text-muted` | `#879596` | Placeholder, empty states |
| `text-link` | `#0073bb` | Links, breadcrumbs |

### Status indicator colors (JS constant `STATUS_COLOR` in `lib/constants.ts`)

| Status | Hex |
|---|---|
| `NoSatEncoding` | `#879596` (muted grey) |
| `Pending` | `#ff9900` (orange) |
| `Running` | `#0073bb` (blue) |
| `Completed` | `#1d8348` (green) |
| `Failed` | `#d13212` (red) |
| `TimedOut` | `#f5a623` (amber) |

Applied via inline `style={{ color, backgroundColor }}`. Never use static Tailwind classes for status colors — they are dynamic.

---

## Layout structure

```
h-screen flex flex-col
├── <header>  min-h-[48px]  bg-[#232f3e]        ← dark nav, sticky top
├── flex flex-1 overflow-hidden
│   ├── <aside>  w-64  bg-white border-r         ← sidebar, fixed width
│   └── <main>   flex-1  bg-[#f2f3f3]            ← content, fills space
└── <footer>  min-h-[36px]  bg-[#fafafa]         ← minimal sticky footer
```

**Critical:** Never add `max-w-*` or `mx-auto` to page content. Content must fill the full main area. The problem detail page uses `flex flex-col h-full` so the page header + content both fill the viewport correctly.

---

## Page structure (ProblemDetailPage pattern)

```
flex flex-col h-full
├── Page header (white bg, border-b, px-6 py-4)  ← title + breadcrumb + action bar
└── Content area (flex-1 overflow-y-auto px-6 py-5 space-y-4)  ← sections
```

Action buttons live in the page header, right-aligned. Order: secondary actions first, primary/destructive last.

---

## Panel (section) convention

Every content section is an AWS-style flat panel — no rounded-xl, no shadow cards.

```tsx
<section className="bg-white border rounded" style={{ borderColor: '#d5dbdb' }}>
  {/* Panel header */}
  <div className="px-4 py-3 border-b" style={{ borderColor: '#d5dbdb', background: '#fafafa' }}>
    <h2 className="text-sm font-semibold" style={{ color: '#16191f' }}>Section title</h2>
  </div>
  {/* Panel body */}
  <div className="px-4 py-4">
    {/* content */}
  </div>
</section>
```

Rules:
- `rounded` only (not `rounded-xl`, not `rounded-lg`)
- Header always has `background: #fafafa`
- No box shadows on panels
- Panels stack vertically with `space-y-4` gap

---

## Table convention

All data tables follow this pattern:

```tsx
<div className="overflow-auto max-h-64 border rounded" style={{ borderColor: '#d5dbdb' }}>
  <table className="w-full text-xs">
    <thead className="sticky top-0" style={{ background: '#fafafa', borderBottom: '1px solid #d5dbdb' }}>
      <tr>
        <th className="text-left px-3 py-2 font-medium" style={{ color: '#545b64' }}>Column</th>
      </tr>
    </thead>
    <tbody>
      {rows.map((row, i) => (
        <tr key={i} style={{ borderTop: '1px solid #eaeded' }}>
          <td className="px-3 py-1.5" style={{ color: '#16191f' }}>{row.value}</td>
        </tr>
      ))}
    </tbody>
  </table>
</div>
```

- `font-mono` on ID/code columns
- Use `text-xs` for all table text
- Sticky thead always uses `#fafafa` background
- Row borders: `#eaeded` (light), not `#d5dbdb`

---

## Component conventions

### Button variants

| Variant | Background | Text | Border | Use for |
|---|---|---|---|---|
| `primary` | `#ff9900` | `#16191f` | `#e88b00` | Main CTA |
| `secondary` | `#ffffff` | `#16191f` | `#aab7b8` | Non-destructive actions |
| `danger` | `#d13212` | `#ffffff` | `#ba2e0f` | Destructive actions |
| `ghost` | transparent | `#545b64` | transparent | Tertiary / toolbar |
| `link` | transparent | `#0073bb` | none | Inline links |

Default size: `md` (`px-4 py-2 text-sm`). Use `sm` (`px-3 py-1 text-xs`) in dense areas.

### Modal

- Gray header (`#f2f3f3` background) with title + × close button
- White body `px-5 py-5`
- Max width `max-w-xl`
- No rounding beyond `rounded`
- Backdrop: `bg-black/50`

### StatusBadge

Dot + text pattern. Both dot and text use the same `STATUS_COLOR` value via inline style. No border, no background.

### ProblemCard (sidebar)

Flat button, full-width, no border-radius card style:
- Left 3px colored stripe = status color (orange when active, status color otherwise)
- Active: `background: #fef6e4`
- Inactive hover: `background: #f2f3f3`
- Name: truncated, `text-sm font-medium`

### Spinner

`border-t-[#ff9900]` (orange) on gray border. Sizes: sm/md/lg.

---

## Sidebar structure

```
w-64 border-r bg-white flex flex-col
├── Header row (fafafa bg): "PROBLEMS" label + Create button
├── Scrollable list of ProblemCards (flex-1 overflow-y-auto)
└── Pagination row (border-t, only when totalPages > 1)
```

Sidebar header "Create" button uses inline hover handlers (not Tailwind hover classes) because Tailwind v4 doesn't support arbitrary value hover states well in SSR-free mode.

---

## Styling rules

1. **Inline styles for dynamic values** — all colors that depend on data (status, satisfiability, hover state) use `style={{}}`. Never use dynamic Tailwind class names (`text-${color}-500` doesn't work with Tailwind v4 JIT purging).

2. **Tailwind for layout/spacing** — use Tailwind classes for flex, grid, padding, margin, width, overflow, positioning.

3. **No Tailwind color classes for brand colors** — use hex values inline or Tailwind arbitrary values like `bg-[#ff9900]` for static brand colors.

4. **No `rounded-xl` or `rounded-lg`** — use `rounded` (4px) only. AWS style is flat.

5. **No box shadows on panels** — only modals get `shadow-2xl`.

6. **Font size scale:**
   - Page title: `text-lg font-semibold`
   - Section heading: `text-sm font-semibold`
   - Body text: `text-sm`
   - Labels/metadata: `text-xs`
   - Table content: `text-xs`

7. **Error color**: `#d13212` (AWS red). Never use `text-red-500`.

8. **Success/completion color**: `#1d8348` (AWS green).

9. **Link color**: `#0073bb` (AWS blue). Used for breadcrumbs and inline links.

---

## Folder structure

```
src/
  lib/
    api.ts          ← CombinatoricsApiService singleton (reads VITE_COMBINATORICS_API_URL)
    constants.ts    ← STATUS_COLOR, SOLVER_OPTIONS, STATUSES_WITH_ENCODING, INSTANCE_TYPES
  hooks/
    useProblems.ts      ← paginated list + refresh
    useProblem.ts       ← single problem + refresh
    useSatEncoding.ts   ← conditional fetch (only when hasEncoding)
    useVariableMap.ts   ← lazy fetch (triggered by user action)
    useSolution.ts      ← solution by instanceType
    useInstance.ts      ← instance data by instanceType
  components/
    Button.tsx           ← primary/secondary/danger/ghost/link variants
    Modal.tsx            ← portal-based, AWS header style
    Spinner.tsx          ← orange spin indicator
    StatusBadge.tsx      ← dot + text, inline status color
    DropdownButton.tsx   ← split button for solver selection
    ProblemCard.tsx      ← flat sidebar nav item with status stripe
    forms/
      BooleanInstanceForm.tsx
      SatInstanceForm.tsx        ← clauses as string[][] (variable names, ~ for negation)
      CspInstanceForm.tsx
      GraphEditorForm.tsx        ← SVG interactive graph editor (Add/Move mode)
      SetInstanceModal.tsx       ← type picker → form orchestrator
    sections/
      ProblemInfoSection.tsx
      DescriptionSection.tsx
      InstanceSection.tsx        ← uses useInstance hook, renders per instanceType
      SatEncodingSection.tsx
      VariableMapSection.tsx     ← lazy load button
      SolutionSection.tsx        ← uses useSolution hook, renders per instanceType
  pages/
    WelcomePage.tsx       ← "/" route, centered empty state
    ProblemDetailPage.tsx ← "/problems/:id", full-width, page header + sections
  services/
    combinatorics/
      CombinatoricsApiService.ts  ← HTTP client, DO NOT MODIFY unless new endpoints added
      types.ts                    ← all request/response DTOs
      index.ts                    ← barrel export
```

---

## API conventions

- `CombinatoricsApiService` is instantiated once as `api` singleton in `lib/api.ts`
- All mutations follow: set loading → call api → on success: close modal + refresh + onProblemChanged → on error: show inline error text
- Error color: `#d13212`
- Error display: `<p className="text-xs" style={{ color: '#d13212' }}>{error}</p>`
- No toast notifications — errors are inline only

### Instance type → API method mapping

| instanceType | GET instance | GET solution | SET instance |
|---|---|---|---|
| `Boolean` | `getBooleanInstance` | `getBooleanSolution` | `setBooleanInstance` |
| `BooleanSatisfiability` | `getSatInstance` | `getSatSolution` | `setSatInstance` |
| `CSP` | `getCspInstance` | `getCspSolution` | `setCspInstance` |
| `VertexColoring` | `getVertexColoringInstance` | `getVertexColoringSolution` | `setVertexColoringInstance` |
| `EdgeColoring` | `getEdgeColoringInstance` | `getEdgeColoringSolution` | `setEdgeColoringInstance` |

### SAT instance format

SET: `clauses: string[][]` — each string is a literal name (`"x1"`) or negated (`"~x1"`).
GET: `clauses: string[][]` — same format.

---

## Triggers

Activate this agent when the request involves: React, component, hook, TypeScript, UI, state, Vite, frontend, SPA, styling, Tailwind, design, page, form, modal, sidebar.
