using System.Drawing;

namespace Lexiconlang.Data.Map;

public class ImageData : CanvasItem {
    public Image imageData;

    public ImageData( string map, Image image, Coordinate location = default( Coordinate ), int layer = 1, int id = -1 ) : base( map, location, layer, id ) {
        imageData = image;
    }
}
