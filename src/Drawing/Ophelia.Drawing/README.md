# Ophelia Drawing

A foundational module for image processing and manipulation within the Ophelia Framework.

## 📂 Featured Tools

### [ImageResizer.cs](./ImageResizer.cs)
A high-level utility for resizing images while maintaining aspect ratio, applying filters, or padding.
- **Key Methods**: `Resize`, `Crop`, `SetFormat`, `ApplyWatermark`.

### [ImageExtensions.cs](./ImageExtensions.cs)
Extension methods for standard `System.Drawing.Image` and `Bitmap` objects to simplify common tasks.
- **Key Methods**: `ToByteArray`, `FromByteArray`, `GetImageFormat`.

## 🖼 Example

```csharp
using Ophelia.Drawing;

Image myImg = Image.FromFile("photo.jpg");
var resized = myImg.Resize(800, 600, ResizeMode.Fit);
resized.Save("photo_small.jpg");
```

---
*Note: This project relies on GDI+ (System.Drawing.Common) or equivalent libraries.*
