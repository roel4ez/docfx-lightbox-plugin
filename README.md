# docfx-lightbox-plugin
A small repository containing a template for DocFx to add lightbox effect for all images.

# Dependencies

## Featherlight
The first version of this template uses the jQuery [Featherlight](https://noelboss.github.io/featherlight/) plugin. Therefore, there is a dependencies on both of these libraries.  
jQuery and the featherlight libraries are loaded from the public CDN.

# Usage
To use this template, you need to add it to your repository, and add the following to your `docfx.json` file:

   "template": [
      "default",
      "templates/lightbox-featherlight"
    ],