# Ophelia Data Attributes

Specialized attributes for decorating data models to control ORM behavior, validation, and relationships.

## 📂 Attribute Categories

### 🛠 Security & Validation
- **[AllowHtml.cs](./AllowHtml.cs)**: Permits HTML content for specific properties.
- **[DisableHtml.cs](./DisableHtml.cs)**: Explicitly forbids HTML to prevent XSS.
- **[DisableMaxLength.cs](./DisableMaxLength.cs)**: Ignores default string length constraints.

### 🔗 Relationships
- **[N2NRelationProperty.cs](./N2NRelationProperty.cs)**: Defines Many-to-Many relationships.
- **[RelationFKProperty.cs](./RelationFKProperty.cs)**: Marks a property as a Foreign Key.
- **[RelationNavigationProperty.cs](./RelationNavigationProperty.cs)**: Marks a navigation property.

### 🏛 Mapping & Logic
- **[DataProperty.cs](./DataProperty.cs)**: General mapping settings for entity properties.
- **[ExcludeDefaultColumn.cs](./ExcludeDefaultColumn.cs)**: Prevents specific columns from being included in default queries.

---
*Fine-tuning data behavior through decoration.*
