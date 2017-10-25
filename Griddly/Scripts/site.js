$(".griddly-filters-form select[multiple]").each(function ()
{
    $(this).multiselect(
        {
            buttonText: function (options, select)
            {
                var display = null;
                var filter = $(select).parents(".griddly-filter-form-group");

                var allItems = $(select).find("option");
                var displayItemCount = parseInt(filter.data("griddly-filter-displayitemcount"));

                if (options.length == allItems.length || (options.length == 0 && filter.data("griddly-filter-isnoneall")))
                    display = (allItems.length == 2 && !filter.data("griddly-filter-isnullable") ? "Both " : "All ") + filter.data("filter-name-plural");
                else if (options.length > displayItemCount)
                    display = options.length + " " + filter.data("filter-name-plural");
                else if (options.length > 0 && options.length <= displayItemCount)
                {
                    var display = $.trim($(options[0]).text());

                    for (var i = 1; i < options.length && i < displayItemCount; i++)
                        display += ", " + $.trim($(options[i]).text());
                }
                else
                    display = "No " + filter.data("filter-name-plural");

                return display;
            },
            enableClickableOptGroups: true,
            isNoneAll: $(this).parents(".griddly-filter-form-group").data("griddly-filter-isnoneall")
        });
});

$(".griddly-filters-form select:not([multiple])").multiselect();