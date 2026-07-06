// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.querySelectorAll("[data-listing-type-form]").forEach(form => {
    const typeSelect = form.querySelector("[data-listing-type]");
    const typeHelp = form.querySelector("[data-listing-type-help]");
    const roommatesField = form.querySelector("[data-roommates-needed-field]");
    const roommatesInput = form.querySelector("[data-roommates-needed]");
    const factsFields = form.querySelector("[data-listing-facts-fields]");

    if (!typeSelect || !typeHelp || !roommatesField || !roommatesInput) {
        return;
    }

    const placeForRentValue = "1";
    const lookingForRoommateValue = "2";

    const updateListingTypeFields = () => {
        const isPlaceForRent = typeSelect.value === placeForRentValue;
        const isLookingForRoommate = typeSelect.value === lookingForRoommateValue;

        typeHelp.textContent = isPlaceForRent
            ? "You have a room, apartment, or house to rent."
            : isLookingForRoommate
                ? "You have or know a place and need one or more roommates."
                : "Choose whether you are listing a place for rent or looking for a roommate.";

        roommatesField.hidden = !isLookingForRoommate;
        roommatesInput.disabled = !isLookingForRoommate;
        roommatesInput.required = isLookingForRoommate;
        factsFields?.classList.toggle("is-place-for-rent", isPlaceForRent);
    };

    typeSelect.addEventListener("change", updateListingTypeFields);
    updateListingTypeFields();
});

document.querySelectorAll("[data-home-gallery]").forEach(gallery => {
    const image = gallery.querySelector("[data-gallery-image]");
    const currentPhoto = gallery.querySelector("[data-gallery-current]");
    const photos = [...gallery.querySelectorAll("[data-gallery-photo]")];
    const openButtons = [...document.querySelectorAll("[data-gallery-open]")];
    const closeButton = gallery.querySelector("[data-gallery-close]");
    const previousButton = gallery.querySelector("[data-gallery-previous]");
    const nextButton = gallery.querySelector("[data-gallery-next]");
    let activeIndex = 0;
    let opener = null;

    if (!image || !currentPhoto || photos.length === 0 || !closeButton) {
        return;
    }

    const showPhoto = index => {
        activeIndex = (index + photos.length) % photos.length;
        const activePhoto = photos[activeIndex];

        image.src = activePhoto.dataset.imageUrl;
        image.alt = activePhoto.dataset.imageAlt;
        currentPhoto.textContent = activeIndex + 1;

        photos.forEach((photo, photoIndex) => {
            const isActive = photoIndex === activeIndex;
            photo.classList.toggle("is-active", isActive);
            photo.setAttribute("aria-pressed", isActive.toString());
        });

        activePhoto.scrollIntoView({
            behavior: "smooth",
            block: "nearest",
            inline: "center"
        });
    };

    const openGallery = event => {
        opener = event.currentTarget;
        gallery.showModal();
        showPhoto(Number(opener.dataset.galleryIndex) || 0);
        closeButton.focus();
    };

    openButtons.forEach(button => {
        button.addEventListener("click", openGallery);
    });

    photos.forEach((photo, photoIndex) => {
        photo.addEventListener("click", () => showPhoto(photoIndex));
    });

    previousButton?.addEventListener("click", () => showPhoto(activeIndex - 1));
    nextButton?.addEventListener("click", () => showPhoto(activeIndex + 1));
    closeButton.addEventListener("click", () => gallery.close());

    gallery.addEventListener("click", event => {
        if (event.target === gallery) {
            gallery.close();
        }
    });

    gallery.addEventListener("keydown", event => {
        if (event.key === "ArrowLeft") {
            event.preventDefault();
            showPhoto(activeIndex - 1);
        }

        if (event.key === "ArrowRight") {
            event.preventDefault();
            showPhoto(activeIndex + 1);
        }
    });

    gallery.addEventListener("close", () => {
        opener?.focus();
    });
});
