use godot::{
    engine::{image::Format, Image},
    prelude::*,
};
use qrcode::QrCode;
use local_ip_address::local_ip;

struct SPJLibExtension;

#[gdextension]
unsafe impl ExtensionLibrary for SPJLibExtension {}

#[derive(GodotClass)]
#[class(base=RefCounted)]
pub struct SPJLib {}

#[godot_api]
impl IRefCounted for SPJLib {
    fn init(_base: Base<RefCounted>) -> Self {
        Self {}
    }
}

#[godot_api]
impl SPJLib {
    #[func]
    fn get_qr_image(&mut self, string: GString) -> Gd<Image> {
        let padding = 1;

        let code = QrCode::new(string.to_string()).unwrap();
        let size: i32 = code.width().try_into().unwrap();
        let mut image =
            Image::create(size + padding * 2, size + padding * 2, false, Format::RGB8).unwrap();
        image.fill(Color::WHITE);

        let mut index: i32 = 0;
        for color in code.to_colors() {
            if color == qrcode::Color::Dark {
                let x = padding + index % size;
                let y = padding + index / size;
                image.fill_rect(
                    Rect2i::new(Vector2i::new(x, y), Vector2i::ONE),
                    Color::BLACK,
                );
            }
            index += 1;
        }

        return image;
    }

    #[func]
    fn get_local_ip() -> String {
        let ip_result = local_ip();
        return match ip_result {
            Ok(ip) => ip.to_string(),
            Err(_err) => String::from("")
        };
    }
}