class TaggedImage {
    constructor(id, image) {
        this.image = image;
        this.tag = id;
        this.internalImage = undefined;
    }

    draw_at(x, y) {
        if (this.image == undefined) {
            alert("Tried to draw an undefined image!");
        }
        const ctx = canvas_get_context();
        ctx.drawImage(this.image, x, y);
    }

    //imageData is in ARGB format
    load_image_data(imageData) {
        if (this.image != undefined) {
            alert("Tried to load an existing image!");
        }

        var b = new Blob([new Uint8Array(imageData)], { type: "image/png" });
        var url = URL.createObjectURL(b);
        this.image = new Image();
        this.image.onload = function () {
            //URL.revokeObjectURL(url);
        }
        this.image.src = url;
        //this.image = createImageBitmap(imageData, 0, 0, height, width);
    }

    destroy() {
        if (this.image != undefined) {
            this.image.close();
            this.image = undefined;
        }
    }
}

var images = new Map(); 
var ctx;
var contexts = [];

var bufferCount = 2;
var nextBuffer = 1;
function canvas_setup() { 
    for (i = 1; i < bufferCount; i++) {
        var x = document.getElementById("drawArea" + i);
        if (x == null) return;
        x.style.visibility = "hidden";
    }
    for (i = 0; i < bufferCount; i++) {
        contexts.push(document.getElementById("drawArea" + i).getContext("2d" ));
    }
    canvas_hide_all();
}
function create_image(id, imageData, sizeX, sizeY) {
    var x = new TaggedImage(id, undefined);
    x.load_image_data(imageData);
    x.image.width = sizeX;
    x.image.height = sizeY;
    images.set(Number(id), x);
    //images.set(id, new TaggedImage(id,));
}

function release_image(id) {
    images.forEach((element, key, map) => {
        if (element.tag == id) {
            element.destroy();
        }
    });
    images.forEach((element, key, map) => {
        if (element.image == undefined) {
            map.delete(key);
        }
    })
}
function canvas_advance_buffer() {
    nextBuffer += 1;
    if (nextBuffer == bufferCount) {
        nextBuffer = 0;
    }
}
function canvas_draw_begin() {
    canvas_advance_buffer();
    canvas_show_one();
    ctx = canvas_get_context(); 
    ctx.fillStyle = "white";
    ctx.fillRect(0, 0, 4000, 4000);
    ctx.strokeStyle = "black";
}
function canvas_draw_end() {
    canvas_show_one(nextBuffer + 1 > bufferCount ? 0 : nextBuffer + 1);
}
function canvas_draw_image(id, locX, locY, scale) {
    var z = images.get(Number(id));
    if (z == undefined) {
        return;
    }
    var x = z.image;
    ctx.drawImage(x, locX, locY, x.width * scale, x.height * scale);
}

function canvas_get_id(id = -1) {
    if (id == -1) {
        id = nextBuffer;
    }

    var name = "drawArea" + id;
    return name;
}

function canvas_resize() {
    for (i = 0; i < bufferCount; i++) {
        const name = canvas_get_id(i);
        const canvas = document.getElementById(name);
        canvas.width = window.innerWidth * 0.85;
        canvas.height = window.innerHeight * 0.80;
        canvas.style.position = "absolute";
        canvas.style.top = Number(-canvas.width);
        canvas.style.left = Number(-canvas.height);
    }
}
function canvas_hide_all() {
    for (i = 0; i < bufferCount; i++) {
        document.getElementById(canvas_get_id(i)).style.visibility = "hidden";
    }
}
function canvas_show_one(id = -1) {
    if (id == -1)
        id = nextBuffer;

    for (i = 0; i < bufferCount; i++) {
        document.getElementById(canvas_get_id(i)).style.visibility = id == i ? "visible" : "hidden";
    }
}
function canvas_get_context() {
    //var ctx = contexts[nextBuffer];
    ctx = document.getElementById("drawArea" + nextBuffer).getContext("2d" ); 
    ctx.lineStyle = "#000000";
    ctx.fillStyle = "#FFFFFF";
    ctx.strokeStyle = "#FFFFFF";
    //ctx.fillRect(0, 0, canvas.width, canvas.height);
    return ctx;
}

function canvas_draw_test() {
    const canvas = document.getElementById(canvas_get_id());
    ctx.fillStyle = "#FFFFFF";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    const near = 36;
    const far = 536;
    canvas_draw_path([[near, near], [near, far], [far, far], [far, near], [near, near]], "#0000FF");
    canvas_draw_end();

    canvas_draw_text("It works! :D", 50, 80);
}

function canvas_draw_text(text, x, y, font = "black bold 24pt sans-serif") {
    ctx.font = font;
    ctx.strokeText(text, x, y);
}

function canvas_draw_end() {
    const canvas = document.getElementById(canvas_get_id());
    canvas_draw_path([[1, 1], [canvas.width - 1, 1], [canvas.width - 1, canvas.height - 1], [1, canvas.height - 1], [1, 1]], "#FF0000");
}

function canvas_draw_path(path, lineColor = "#0000FF", width = 1) {
    ctx.strokeStyle = lineColor;
    ctx.lineWidth = width;

    try {
        ctx.beginPath();
        ctx.moveTo(path[0][0], path[0][1]);
        path.forEach((element) => ctx.lineTo(element[0], element[1]));

        ctx.stroke();
    }
    catch {

    }
}
