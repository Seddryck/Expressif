# To install magick
# winget install ImageMagick.ImageMagick

# To build the .ico from two distinct resolutions
magick `
  ../misc/icon/expressif-icon-256.png `
  `( ../misc/icon/expressif-icon-256.png -resize 128x128 `) `
  ../misc/icon/expressif-icon-64.png `
  `( ../misc/icon/expressif-icon-64.png -resize 48x48 `) `
  `( ../misc/icon/expressif-icon-64.png -resize 32x32 `) `
  `( ../misc/icon/expressif-icon-64.png -resize 24x24 `) `
  `( ../misc/icon/expressif-icon-64.png -resize 16x16 `) `
  expressif.ico

# To display the different frames
# magick expressif.ico expressif-frame-%02d.png