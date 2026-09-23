# CSS

Plain CSS on top of Bootstrap 5.3, loaded in this order from `Views/Shared/_Layout.cshtml`:

| File | What goes in it |
| --- | --- |
| `base.css` | Design tokens (`--cm-*`), element defaults, focus ring, keyframes, reduced motion |
| `layout.css` | Containers, header, navigation, account menu, notification bell, page shells |
| `components.css` | Anything used on more than one page: buttons, forms, alerts, pills, chips, empty states, page headers, listing card, photo viewer, pager, scroll animations |
| `pages/*.css` | One file per page or page area, named after it |

Order matters only in that direction: a page file may override a component, never the other way round.

## Conventions

- Use the tokens from `base.css` for colours, radii and shadows instead of raw values.
- Class names are prefixed by the component or page they belong to (`listing-card-*`, `roommate-card-*`, `profile-hero-*`).
- Responsive rules sit at the bottom of each file, desktop first: `max-width: 1199.98px`, `991.98px`, `767.98px`, `575.98px`.
- Avoid `!important`. To change Bootstrap's navbar or dropdown spacing, set its CSS variables (`--bs-nav-link-padding-x`, `--bs-dropdown-spacer`) instead of out-specifying it.
- If a style is needed on a second page, move it to `components.css` rather than copying it.
