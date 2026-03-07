# Ophelia Globalization

This module provides tools for internationalization and localization, specifically focusing on complex linguistic transformations.

## 📂 Number to Words Conversion

The **[NumberToWords](./NumberToWords)** directory contains specialized converters that transform numeric values into their localized text representations. This is essential for financial applications, invoice generation, and report writing.

### Supported Languages & Regions:
- **English**: [American](./NumberToWords/AmericanConverter.cs), [British](./NumberToWords/BritishConverter.cs).
- **Mediterranean**: [Spanish](./NumberToWords/SpanishConverter.cs), [French](./NumberToWords/FrenchConverter.cs), [Arabic](./NumberToWords/ArabicConverter.cs), [Hebrew](./NumberToWords/HebrewConverter.cs).
- **European**: [German](./NumberToWords/GermanConverter.cs), [Dutch](./NumberToWords/DutchConverter.cs), [Polish](./NumberToWords/PolishConverter.cs), [Serbian](./NumberToWords/SerbianConverter.cs).
- **Eurasian**: [Turkish](./NumberToWords/TurkishConvertor.cs), [Russian](./NumberToWords/RussianConverter.cs), [Ukrainian](./NumberToWords/UkrainianConverter.cs), [Uzbek](./NumberToWords/UzbekConverter.cs).

## 🛠 Usage

```csharp
using Ophelia.Globalization.NumberToWords;

decimal amount = 1234.56m;
string inWords = amount.ToWords(new TurkishConvertor());
// Output: "Bin İki Yüz Otuz Dört Lira Elli Altı Kuruş" (context dependent)
```
---
*Bridging languages through code.*
