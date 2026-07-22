document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('[data-gallery]').forEach((gallery) => {
    const mainImage = gallery.querySelector('.gallery-main-image');
    const thumbs = Array.from(gallery.querySelectorAll('.gallery-thumb'));

    if (!mainImage || thumbs.length === 0) {
      return;
    }

    const setActive = (thumb) => {
      thumbs.forEach((button) => {
        const active = button === thumb;
        button.classList.toggle('is-active', active);
        button.setAttribute('aria-pressed', active ? 'true' : 'false');
      });

      const nextSrc = thumb.getAttribute('data-src');
      const nextAlt = thumb.getAttribute('data-alt') || '';

      if (nextSrc) {
        mainImage.src = nextSrc;
      }

      mainImage.alt = nextAlt;
    };

    thumbs.forEach((thumb) => {
      thumb.setAttribute('aria-pressed', thumb.classList.contains('is-active') ? 'true' : 'false');
      thumb.addEventListener('click', () => setActive(thumb));
    });

    const initial = thumbs.find((thumb) => thumb.classList.contains('is-active')) || thumbs[0];
    setActive(initial);
  });
});
