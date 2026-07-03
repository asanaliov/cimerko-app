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
