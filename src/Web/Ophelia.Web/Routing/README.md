# Ophelia Web Routing

Advanced routing management for ASP.NET Core, allowing for high-performance and dynamic URL resolution.

## 📂 Components

### [RouteCollection.cs](./RouteCollection.cs)
A centralized collection of all application routes, supporting pattern matching and fast lookup.

### [RouteHandler.cs](./RouteHandler.cs) & [CustomRouteHandler.cs](./CustomRouteHandler.cs)
Logic that decides which controller or action should handle a given request based on the path.

### [RouteItem.cs](./RouteItem.cs)
Represents a single route entry, including its URL pattern, parameters, and target action.

## 📁 URL Logic
- **[RouteItemURL.cs](./RouteItemURL.cs)**: Basic URL routing.
- **[RouteItemURLPattern.cs](./RouteItemURLPattern.cs)**: Regex or pattern-based routing.
- **[RouteItemFixedURL.cs](./RouteItemFixedURL.cs)**: Static path routing.

---
*Sophisticated URL management for modern SEO needs.*
