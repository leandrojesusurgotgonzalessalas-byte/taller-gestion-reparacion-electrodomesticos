# Logo del taller

Se utilizó el símbolo entregado por Leandro, eliminando las palabras inferiores y adaptando sus tonos al diseño de la web. Se conservaron el techo, la chimenea, los contornos estructurales, la ventana, los electrodomésticos y el cable con enchufe.

- Maestro: `src/Taller.Web/wwwroot/branding/taller-logo.png`, con transparencia.
- Windows: `src/Taller.Web/wwwroot/branding/taller.ico`, con resoluciones de 16 a 256 píxeles.
- Web: favicon ICO, PNG de 192 y 512 píxeles y copia de compatibilidad `wwwroot/favicon.ico`.
- Uso: navegación lateral, encabezado, menú móvil, pestaña del navegador, ejecutables y acceso directo del escritorio.
- Instalación del acceso: `scripts/InstalarAccesoDirecto.ps1`. El acceso abre el mismo `Iniciar.cmd` de la aplicación y conserva el anterior iniciador del escritorio en `runtime`.

Paleta: verde salvia #4A6B57, verde oscuro #2F4739 y gris cálido #A6A99D, sobre el beige #F4F1EA de la web.

La edición gráfica se realizó con el generador de imágenes integrado. Pillow se usó únicamente para convertir formatos y resoluciones del resultado, conservando su canal alfa. El original se conserva en `docs/IdentidadVisual/logo-original.png`.

## Prompt inicial

Use case: precise-object-edit / logo-brand. Edit target: the supplied appliance repair logo. Remove ONLY the two bottom words 'electrodomésticos' and 'hogar'. Preserve exactly the original recognizable symbol: house roof and chimney, intertwined rounded CR-like structural outlines, four-pane window, washing machine, refrigerator, microwave, and bottom cable with plug. Do not interpret the structural CR-like shapes as lettering to remove; preserve them. Recolor the deep blue to muted sage green #4A6B57 and dark sage #2F4739, and replace shiny silver with subtle warm gray/beige #A6A99D to harmonize with a warm beige #F4F1EA minimal website. Keep the same composition and all original symbol components, crisp clean edges, very subtle depth only, no strong shine or shadows. Genuine transparent background, including empty spaces within the outline; no white rectangular background. Center the text-free symbol in a square canvas with small balanced padding, maximize usable size for a website logo and Windows app icon. No words, no captions, no new letters, no watermark.

## Prompt de la edición final

Precise cleanup edit of the supplied text-free logo. Keep all existing shapes and composition unchanged. Remove ALL yellow, lime-green, white, black, or noisy fringes/speckles around contours and in the empty transparent areas. Produce perfectly clean antialiased edges and a fully transparent background including interiors. Use only matte muted sage #4A6B57 / dark sage #2F4739 for the symbol and warm gray #A6A99D for fine outlines and appliance details. No glow, no drop shadow, no grunge, no halos, no metallic gradient. Preserve house roof, chimney, CR-like structural outlines, window, washer, refrigerator, microwave, cable and plug exactly. No words or added text. Square application icon composition with balanced padding.
