"""Generate the glossy QR hunt badge artwork used by the demo map.

Requires Pillow. Outputs transparent 512 px PNG sprites into
Assets/Resources/Artwork/StationBadges.
"""

from pathlib import Path
import math
from PIL import Image, ImageDraw, ImageFont, ImageFilter


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/Resources/Artwork/StationBadges"
S = 1024
RATIO = S / 512


def p(value):
    return int(round(value * RATIO))


def font(size, heavy=True):
    candidates = [
        r"C:\Windows\Fonts\seguisb.ttf" if heavy else r"C:\Windows\Fonts\segoeui.ttf",
        r"C:\Windows\Fonts\arialbd.ttf" if heavy else r"C:\Windows\Fonts\arial.ttf",
    ]
    for path in candidates:
        try:
            return ImageFont.truetype(path, p(size))
        except OSError:
            pass
    return ImageFont.load_default()


def radial(size, center, radius, inner, outer):
    image = Image.new("RGBA", (size, size))
    px = image.load()
    cx, cy = center
    for y in range(size):
        for x in range(size):
            t = min(1.0, math.hypot(x - cx, y - cy) / radius)
            t = t * t * (3 - 2 * t)
            px[x, y] = tuple(round(inner[i] * (1 - t) + outer[i] * t) for i in range(4))
    return image


def vertical_gradient(size, top, bottom):
    image = Image.new("RGBA", (size, size))
    draw = ImageDraw.Draw(image)
    for y in range(size):
        t = y / max(1, size - 1)
        color = tuple(round(top[i] * (1 - t) + bottom[i] * t) for i in range(4))
        draw.line((0, y, size, y), fill=color)
    return image


def shadow_layer(shape_fn):
    layer = Image.new("RGBA", (S, S))
    shape_fn(ImageDraw.Draw(layer), (7, 16))
    return layer.filter(ImageFilter.GaussianBlur(p(12)))


def save(image, name):
    image = image.resize((512, 512), Image.Resampling.LANCZOS)
    image.save(OUT / f"{name}.png", optimize=True)


def sparkle(draw, x, y, size, color):
    x, y, size = p(x), p(y), p(size)
    draw.polygon([(x, y - size), (x + size // 4, y - size // 4),
                  (x + size, y), (x + size // 4, y + size // 4),
                  (x, y + size), (x - size // 4, y + size // 4),
                  (x - size, y), (x - size // 4, y - size // 4)], fill=color)


def make_frame():
    image = Image.new("RGBA", (S, S))
    image.alpha_composite(shadow_layer(lambda d, o: d.ellipse(
        (p(50 + o[0]), p(50 + o[1]), p(462 + o[0]), p(462 + o[1])), fill=(1, 5, 24, 255))))
    d = ImageDraw.Draw(image)
    d.ellipse((p(42), p(42), p(470), p(470)), fill=(9, 14, 48, 255), outline=(4, 5, 24, 255), width=p(8))
    d.ellipse((p(51), p(51), p(461), p(461)), outline=(23, 240, 255, 255), width=p(7))
    d.ellipse((p(62), p(62), p(450), p(450)), outline=(255, 58, 159, 235), width=p(4))
    d.ellipse((p(72), p(72), p(440), p(440)), fill=(18, 26, 66, 238), outline=(255, 255, 255, 85), width=p(2))
    # Glossy highlights make the frame read like a polished game token.
    d.arc((p(64), p(59), p(448), p(443)), 196, 326, fill=(255, 255, 255, 205), width=p(8))
    d.arc((p(74), p(69), p(438), p(433)), 18, 118, fill=(255, 255, 255, 95), width=p(5))
    d.arc((p(48), p(47), p(464), p(463)), 205, 306, fill=(255, 231, 99, 255), width=p(5))
    sparkle(d, 95, 150, 11, (255, 255, 255, 245))
    sparkle(d, 420, 360, 9, (85, 245, 255, 245))
    save(image, "BadgeFrame")


def qr_finder(draw, x, y, module, color):
    # Three rounded finder marks evoke a QR without representing a real payload.
    x, y, m = p(x), p(y), p(module)
    draw.rounded_rectangle((x, y, x + 7*m, y + 7*m), radius=p(4), fill=color)
    draw.rounded_rectangle((x + m, y + m, x + 6*m, y + 6*m), radius=p(2), fill=(16, 24, 62, 255))
    draw.rounded_rectangle((x + 2*m, y + 2*m, x + 5*m, y + 5*m), radius=p(2), fill=color)


def make_qr(station_id, accent, secondary):
    image = Image.new("RGBA", (S, S))
    glow = Image.new("RGBA", (S, S))
    gd = ImageDraw.Draw(glow)
    gd.ellipse((p(64), p(64), p(448), p(448)), fill=(*accent, 105))
    image.alpha_composite(glow.filter(ImageFilter.GaussianBlur(p(24))))

    # Dark enamel tile with a raised rim and soft specular light.
    d = ImageDraw.Draw(image)
    d.rounded_rectangle((p(100), p(93), p(412), p(419)), radius=p(62), fill=(4, 8, 32, 255), outline=(0, 0, 0, 180), width=p(8))
    tile = vertical_gradient(S, (46, 65, 122, 255), (12, 20, 57, 255))
    mask = Image.new("L", (S, S)); ImageDraw.Draw(mask).rounded_rectangle((p(110), p(102), p(402), p(409)), radius=p(54), fill=255)
    image.alpha_composite(Image.composite(tile, Image.new("RGBA", (S, S)), mask))
    d = ImageDraw.Draw(image)
    d.rounded_rectangle((p(110), p(102), p(402), p(409)), radius=p(54), outline=(*accent, 255), width=p(7))
    d.arc((p(122), p(111), p(390), p(379)), 195, 334, fill=(255, 255, 255, 180), width=p(6))

    # Decorative QR modules in a bright scanning plate.
    d.rounded_rectangle((p(152), p(145), p(360), p(353)), radius=p(27), fill=(239, 251, 255, 255), outline=(255, 255, 255, 255), width=p(5))
    module = 10
    for xx, yy in [(164, 157), (291, 157), (164, 284)]:
        qr_finder(d, xx, yy, module, (*accent, 255))
    # Deterministic decorative modules. A question medallion intentionally interrupts the pattern.
    for row in range(15):
        for col in range(15):
            x, y = 164 + col*module, 157 + row*module
            in_finder = (col < 7 and row < 7) or (col > 7 and row < 7) or (col < 7 and row > 7)
            center = 4 <= col <= 10 and 4 <= row <= 10
            if in_finder or center:
                continue
            if ((row*7 + col*11 + station_id*13) % 9) in (0, 1, 3, 5):
                d.rounded_rectangle((p(x+2), p(y+2), p(x+8), p(y+8)), radius=p(2), fill=(18, 30, 72, 255))

    # Center coin with a question mark: the visible badge promises a discovery, not a real scan.
    d.ellipse((p(199), p(192), p(321), p(314)), fill=(8, 17, 49, 255), outline=(255, 255, 255, 255), width=p(6))
    d.ellipse((p(207), p(200), p(313), p(306)), fill=(*accent, 255), outline=(*secondary, 255), width=p(4))
    d.arc((p(211), p(203), p(309), p(301)), 190, 320, fill=(255, 255, 255, 220), width=p(5))
    d.text((p(236), p(211)), "?", font=font(76), fill=(255, 255, 255, 255), stroke_width=p(2), stroke_fill=(12, 24, 62, 255))
    # Small scan glints at the tile corners.
    sparkle(d, 128, 119, 8, (255, 255, 255, 230))
    sparkle(d, 385, 390, 7, (*accent, 255))
    save(image, f"QRStation_{station_id}")


def treasure(opened):
    image = Image.new("RGBA", (S, S))
    glow = Image.new("RGBA", (S, S))
    gd = ImageDraw.Draw(glow)
    gd.ellipse((p(92), p(74), p(420), p(414)), fill=(255, 187, 37, 135 if opened else 80))
    image.alpha_composite(glow.filter(ImageFilter.GaussianBlur(p(34))))
    d = ImageDraw.Draw(image)

    # Shadow below the treasure.
    d.ellipse((p(119), p(390), p(393), p(437)), fill=(3, 7, 32, 150))
    if opened:
        # Floating lid behind the chest, rotated slightly for a celebratory reveal.
        lid = Image.new("RGBA", (p(245), p(103)))
        ld = ImageDraw.Draw(lid)
        ld.rounded_rectangle((p(4), p(5), p(241), p(98)), radius=p(35), fill=(120, 48, 19, 255), outline=(255, 224, 111, 255), width=p(8))
        ld.rectangle((p(28), p(22), p(216), p(52)), fill=(255, 182, 47, 255))
        lid = lid.rotate(18, resample=Image.Resampling.BICUBIC, expand=True)
        image.alpha_composite(lid, (p(195), p(74)))

    # Chest body gradient and dark bevel.
    d = ImageDraw.Draw(image)
    d.rounded_rectangle((p(91), p(202), p(421), p(404)), radius=p(46), fill=(71, 27, 21, 255), outline=(21, 8, 25, 255), width=p(12))
    chest = vertical_gradient(S, (255, 200, 61, 255), (218, 108, 26, 255))
    mask = Image.new("L", (S,S)); ImageDraw.Draw(mask).rounded_rectangle((p(105), p(214), p(407), p(389)), radius=p(33), fill=255)
    image.alpha_composite(Image.composite(chest, Image.new("RGBA", (S,S)), mask))
    d = ImageDraw.Draw(image)
    d.rounded_rectangle((p(105), p(214), p(407), p(389)), radius=p(33), outline=(255, 225, 127, 255), width=p(7))
    d.rounded_rectangle((p(118), p(225), p(394), p(261)), radius=p(15), fill=(255, 227, 137, 255))
    d.rounded_rectangle((p(176), p(213), p(226), p(392)), radius=p(12), fill=(255, 231, 142, 255), outline=(179, 89, 23, 255), width=p(4))
    d.rounded_rectangle((p(285), p(213), p(329), p(392)), radius=p(11), fill=(255, 190, 61, 255), outline=(179, 89, 23, 255), width=p(4))
    if opened:
        # Light beam and gems rising from the chest.
        beam = Image.new("RGBA", (S, S))
        bd = ImageDraw.Draw(beam)
        bd.polygon([(p(166), p(246)), (p(358), p(246)), (p(320), p(100)), (p(206), p(100))], fill=(255, 222, 83, 75))
        image.alpha_composite(beam.filter(ImageFilter.GaussianBlur(p(14))))
        d = ImageDraw.Draw(image)
        d.polygon([(p(194), p(205)), (p(218), p(148)), (p(248), p(193)), (p(278), p(128)), (p(316), p(205))], fill=(22, 219, 232, 255), outline=(226, 255, 255, 255))
        d.polygon([(p(218), p(148)), (p(244), p(165)), (p(248), p(193))], fill=(151, 255, 245, 255))
        d.polygon([(p(278), p(128)), (p(291), p(172)), (p(316), p(205))], fill=(255, 79, 163, 255))
        sparkle(d, 131, 166, 18, (255, 247, 173, 255))
        sparkle(d, 380, 168, 15, (255, 255, 255, 255))
        sparkle(d, 155, 322, 10, (255, 255, 255, 255))
    else:
        # Closed domed lid and a clear lock mark.
        d = ImageDraw.Draw(image)
        d.rounded_rectangle((p(111), p(145), p(401), p(244)), radius=p(47), fill=(232, 141, 32, 255), outline=(255, 223, 120, 255), width=p(8))
        d.arc((p(119), p(134), p(393), p(259)), 184, 357, fill=(255, 245, 194, 255), width=p(8))
        d.rounded_rectangle((p(224), p(288), p(289), p(349)), radius=p(17), fill=(116, 53, 23, 255), outline=(255, 237, 170, 255), width=p(5))
        d.arc((p(232), p(265), p(281), p(317)), 180, 360, fill=(255, 243, 182, 255), width=p(11))
        d.rounded_rectangle((p(214), p(286), p(298), p(361)), radius=p(20), fill=(255, 244, 185, 255), outline=(145, 63, 24, 255), width=p(8))
        d.ellipse((p(241), p(301), p(272), p(332)), fill=(93, 40, 27, 255))
        d.rounded_rectangle((p(249), p(324), p(264), p(350)), radius=p(6), fill=(93, 40, 27, 255))
    save(image, "PrizeReady" if opened else "PrizeLocked")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    make_frame()
    accents = [
        ((29, 225, 245), (117, 255, 220)),
        ((255, 65, 156), (255, 189, 81)),
        ((111, 242, 92), (214, 255, 111)),
        ((255, 146, 47), (255, 228, 98)),
        ((81, 161, 255), (132, 253, 255)),
    ]
    for station_id, (accent, secondary) in zip(range(2, 7), accents):
        make_qr(station_id, accent, secondary)
    treasure(False)
    treasure(True)
    for path in sorted(OUT.glob("*.png")):
        print(f"{path.name}: {path.stat().st_size / 1024:.1f} KB")


if __name__ == "__main__":
    main()
