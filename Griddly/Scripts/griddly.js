/*
 * Griddly script
 * http://griddly.com
 * Copyright 2013-2014 Chris Hynes and Data Research Group, Inc.
 * Licensed under MIT (https://github.com/programcsharp/griddly/blob/master/LICENSE)
 *
 * WARNING: Don't edit this file -- it'll be overwitten when you upgrade.
 *
 */

!function ($)
{
    "use strict"; // jshint ;_;

    var Griddly = function(element, options)
    {
        this.$element = $(element);
        this.options = options;
        this.create();

        $(this.$element).find("[data-enable-on-selection=true]").addClass("disabled");

        if (this.options.onRefresh)
            this.options.onRefresh(this, 0, this.options.count > this.options.pageSize ? this.options.pageSize : this.options.count, this.options.count, null);

        this.$element.trigger("init.griddly", 
        {
            start: 0,
            pageSize: this.options.count > this.options.pageSize ? this.options.pageSize : this.options.count,
            count: this.options.count
        });

        $("html").on("click", $.proxy(function (event)
        {
            if ($(event.target).parents('.popover.in').length == 0 && $(event.target).parents(".filter-trigger").length == 0 && !$(event.target).hasClass("filter-trigger"))
            {
                $(".griddly-filters-inline .filter-trigger").popover("hide");
            }
        }, this));

        this.setSelectedCount = $.proxy(function ()
        {
            $("thead tr .griddly-selection-count", this.$element).text(Object.keys(this.options.selectedRows).length);

            if (!$.isEmptyObject(this.options.selectedRows))
            {
                this.$element.find("tr.griddly-selection:not(:visible)").show("slow");
                $(this.$element).find("[data-enable-on-selection=true]").removeClass("disabled");
            }
            else
            {
                this.$element.find("tr.griddly-selection:visible").hide("slow");
                $(this.$element).find("[data-enable-on-selection=true]").addClass("disabled");
            }

        }, this);
    };

    var serializeObject = function ($elements)
    {
        // http://stackoverflow.com/a/1186309/8037
        var data = {};

        $.each($elements.serializeArray(), function ()
        {
            if (data[this.name] !== undefined)
            {
                if (!data[this.name].push)
                    data[this.name] = [data[this.name]];

                data[this.name].push(this.value || '');
            }
            else
            {
                data[this.name] = this.value || '';
            }
        });

        return data;
    };

    Griddly.prototype = {
        constructor: Griddly,

        // create and bind
        create: function ()
        {
            var url = this.$element.data("griddly-url");
            var count = this.$element.data("griddly-count");
            var pageSize = this.$element.data("griddly-pagesize");
            var defaultSort = this.$element.data("griddly-defaultsort");
            var defaultRowIds = this.$element.data("griddly-defaultrowids");
            var isMultiSort = this.$element.data("griddly-multisort");
            var onRefresh = this.$element.data("griddly-onrefresh");
            var rowClickModal = this.$element.data("griddly-rowclickmodal");

            this.options.url = url;
            this.options.defaultRowIds = defaultRowIds;
            this.options.count = parseInt(count);
            if (pageSize)
                this.options.pageSize = parseInt(pageSize);
            this.options.pageCount = this.options.count * this.options.pageSize;
            this.options.rowClickModal = rowClickModal;

            if (!this.options.selectedRows) {
                this.options.selectedRows = {};
            }            

            if (isMultiSort != null)
                this.options.isMultiSort = isMultiSort == true;

            this.options.sortFields = defaultSort && defaultSort.length ? defaultSort : [];

            if (onRefresh && Object.prototype.toString.call(window[onRefresh]) == '[object Function]')
                this.options.onRefresh = window[onRefresh];

            // TODO: should we do this later on so we handle dynamically added buttons?
            this.$element.find("[data-append-rowids-to-url]").each(function ()
            {
                $(this).data("griddly-href-template", $(this).attr("href"));
            });

            $("form", this.$element).attr("onsubmit", "return false;");

            $("a.next", this.$element).on("click", $.proxy(function (event)
            {
                this.pageNumber(this.options.pageNumber + 1);

                this.refresh();

                event.preventDefault();
            }, this));

            $("a.prev", this.$element).on("click", $.proxy(function (event)
            {
                this.pageNumber(this.options.pageNumber - 1);

                this.refresh();

                event.preventDefault();
            }, this));

            $("input.pageNumber", this.$element).on("change", $.proxy(function (event)
            {
                var value = parseInt($(event.target).val());

                if (value < 1)
                    value = 1;
                else if (value > this.options.pageCount)
                    value = this.options.pageCount;

                this.options.pageNumber = value - 1;

                this.refresh();
            }, this));

            $("select.pageSize", this.$element).on("change", $.proxy(function (event)
            {
                var value = parseInt($(event.target).val());

                if (value < 1)
                    value = 1;
                else if (value > 1000)
                    value = 1000;

                this.options.pageNumber = Math.floor(this.options.pageNumber * this.options.pageSize / value);
                this.options.pageSize = value;

                this.refresh();
            }, this));
             
            $("form", this.$element).on("submit", $.proxy(function (event)
            {
                this.refresh(true);

                event.preventDefault();
            }, this));

            $("form .grid_searchreset", this.$element).on("click", $.proxy(function (event)
            {
                this.$element.find("form .transient").remove();
                this.$element.find("form")[0].reset();

                this.refresh(true);
            }, this));

            $("a.btn-search", this.$element).on("click", $.proxy(function (event)
            {
                this.$element.find("tr.griddly-filters").toggle();
            }, this));

            $(this.$element).on("mouseup", "tbody.data tr td:not(:has(input))", $.proxy(function (e)
            {
                var url = $(e.target).parents("tr").data("griddly-url");

                if (url && $(e.target).closest("a").length == 0)
                {
                    if (this.options.rowClickModal)
                    {
                        $(this.options.rowClickModal).removeData("modal").modal({ remote: url });
                    }
                    else
                    {
                        if (e.which == 2 || e.ctrlKey)
                            window.open(url);
                        else if (e.which != 3)
                            window.location = url;
                    }

                    e.preventDefault();
                }
            }, this));

            $(this.$element).on("mousedown", "tbody.data tr td:not(:has(input))", $.proxy(function (e)
            {
                var url = $(e.target).parents("tr").data("griddly-url");

                if (url)
                    e.preventDefault();
            }, this));

            $(this.$element).on("click", "thead tr.columnHeaders th", $.proxy(function (event)
            {
                var sortField = $(event.currentTarget).data("griddly-sortfield");

                if (sortField)
                {
                    var currentPos = -1;

                    for (var i = 0; i < this.options.sortFields.length; i++)
                    {
                        if (this.options.sortFields[i].Field == sortField)
                        {
                            currentPos = i;

                            break;
                        }
                    }

                    var currentDirection = currentPos != -1 ? this.options.sortFields[currentPos].Direction : "Descending";
                    var newDirection = currentDirection == "Descending" ? "Ascending" : "Descending";

                    var inlineFilters = $("tr.griddly-filters-inline", this.element);

                    if (!this.options.isMultiSort || !event.ctrlKey)
                    {
                        for (var i = 0; i < this.options.sortFields.length; i++)
                        {
                            var thisSortField = this.options.sortFields[i].Field;

                            if (thisSortField != sortField)
                            {
                                var oldSortDisplay = $(event.currentTarget).parents("tr").find("th[data-griddly-sortfield='" + thisSortField + "']");

                                if (inlineFilters.length)
                                    oldSortDisplay = [oldSortDisplay[0], inlineFilters[0].cells[oldSortDisplay[0].cellIndex]];

                                $(oldSortDisplay).removeClass("sorted_a").removeClass("sorted_d");
                            }
                        }

                        this.options.sortFields = [];
                    }

                    if (currentPos != -1 && this.options.sortFields.length)
                        this.options.sortFields[currentPos].Direction = newDirection;
                    else
                        this.options.sortFields.push({ Field: sortField, Direction: newDirection });


                    var newSortDisplay = [event.currentTarget];

                    if (inlineFilters.length)
                        newSortDisplay.push(inlineFilters[0].cells[newSortDisplay[0].cellIndex]);

                    if (newDirection == "Ascending")
                        $(newSortDisplay).removeClass("sorted_d").addClass("sorted_a");
                    else
                        $(newSortDisplay).removeClass("sorted_a").addClass("sorted_d");

                    this.refresh(true);
                }
            }, this));

            var onRowChange = $.proxy(function (event)
            {
                this.setSelectedCount();

                this.$element.find("[data-append-rowids-to-url]").each(function ()
                {
                    var template = $(this).data("griddly-href-template");

                    if (template)
                    {
                        var selection = this.getSelected($(this).data["rowids"]);
                        var query = [];
                        for (var k in selection)
                        {
                            query[query.length] = k + "=" + selection[k].join(",");
                        }

                        $(this).attr("href", template + (template.indexOf("?") > -1 ? "&" : "?") + query.join("&"));
                    }            
                });
            }, this);

            var setRowSelect = $.proxy(function ($checkbox)
            {
                var rowkey = $checkbox.data("rowkey");                
                var row = this.options.selectedRows[rowkey];

                if (!row && $checkbox.prop("checked"))
                {
                    var rowvalue = { "value": $checkbox.attr("value") };
                    var data = $checkbox.data();                                        
                    for (var d in data)
                        rowvalue[d] = data[d];

                    this.options.selectedRows[rowkey] = rowvalue;
                }
                else if (row && !$checkbox.prop("checked"))
                {
                    delete this.options.selectedRows[rowkey];
                }

                onRowChange();
            }, this);

            $(this.$element).on("change", "input[name=_rowselect]", onRowChange);

            $(this.$element).on("click", "td.griddly-select", $.proxy(function (event)
            {
                var $target = $(event.target);

                var $checkbox = $target;
                if (!$target.is("input")) {
                    $checkbox = $target.find("input[name=_rowselect]");

                    $checkbox.prop("checked", !$checkbox.prop("checked"));
                }

                setRowSelect($checkbox);

                if (event.shiftKey && this.options.lastSelectedRow)
                {
                    var last = $("tbody tr", this.$element).index(this.options.lastSelectedRow);
                    var first = $("tbody tr", this.$element).index($target.parents("tr"));

                    var start = Math.min(first, last);
                    var end = Math.max(first, last);

                    $("tbody tr", this.$element).slice(start, end).find("input[name=_rowselect]").prop("checked", true);
                }

                this.options.lastSelectedRow = $target.parents("tr");
            }, this));

            $(this.$element).on("click", "thead tr.columnHeaders th.select", $.proxy(function (event)
            {
                if (this.$element.find("input[name=_rowselect]:not(:checked)").length == 0)
                    this.$element.find("input[name=_rowselect]").prop("checked", false).each(function() { setRowSelect($(this)); });
                else
                    this.$element.find("input[name=_rowselect]").prop("checked", true).each(function () { setRowSelect($(this)); });

                onRowChange();
            }, this));
            
            $(this.$element).on("click", "thead tr .griddly-selection-clear", $.proxy(function (event)
            {
                this.options.selectedRows = {};

                $("tbody tr", this.$element).find("input[name=_rowselect]").prop("checked", false);

                onRowChange();
                this.setSelectedCount();
            }, this));

            $("a.export-xlsx", this.$element).on("click", $.proxy(function (e) {
                this.exportFile("xlsx");
                e.preventDefault();
            }, this));
            $("a.export-csv", this.$element).on("click", $.proxy(function (e) {
                this.exportFile("csv");
                e.preventDefault();
            }, this));
            $("a.export-tsv", this.$element).on("click", $.proxy(function (e) {
                this.exportFile("tsv");
                e.preventDefault();
            }, this));

            $(".griddly-filters-inline .griddly-filter .griddly-filter-clear", this.$element).click(function (e)
            {
                $(this).parents(".input-group").find("input").val("").focus().triggerHandler("change", [true]);/*.on("blur", function()
                {
                    $(this).detach("blur");

                    if (!$(this).val())
                        $(this).change();
                });*/
            });

            var getFormattedValue = function (val, dataType)
            {
                switch (dataType)
                {
                    case "Integer":
                    {
                        val = new String(val).replace(/[^0-9]/g, "")

                        val = parseInt(val);

                        if (!isFinite(val))
                            val = null;

                        return val;
                    }
                    case "Decimal":
                    case "Currency":
                    //case "Percent":
                        val = new String(val).replace(/,/g, "").replace(/\$/g, "");

                        // TODO: filter down to one decimal point
                        // TODO: filter out non numerics
                        val = parseFloat(val);

                        if (!isFinite(val))
                            val = null;
                        else
                        {
                            val = val.toFixed(2)

                            var x = val.split('.');

                            var x1 = x[0];

                            var x2 = x.length > 1 ? '.' + x[1] : '';

                            var rgx = /(\d+)(\d{3})/;

                            while (rgx.test(x1))
                            {
                                x1 = x1.replace(rgx, '$1' + ',' + '$2');
                            }

                            val = x1 + x2;
                        }

                        if (dataType == "Currency")
                            val = "$" + val;
                        //else if (dataType == "Percent")
                        //    val += "%";

                        if (val.indexOf(".00", val.length - 3) !== -1)
                            val = val.substr(0, val.length - 3);

                        return val;
                    case "Date":
                        // TODO: handle bad formats
                        val = new Date(val);

                        return (val.getMonth() + 1) + "/" + val.getDate() + "/" + val.getFullYear();
                    default:
                        return val;
                }
            };

            var isNullOrWhiteSpace = function (str)
            {
                return str == null || str.match(/^ *$/) !== null;
            }

            var trimToNull = function (str)
            {
                if (isNullOrWhiteSpace(str))
                    return null;
                else
                    return str;
            }

            this.$inlineFilters = $(".griddly-filters-inline .filter-content input", this.$element);

            $(".griddly-filters-inline .filter-content input", this.$element).on("change", $.proxy(function (event, dontHide)
            {
                var filter = $(event.currentTarget).data("griddly-filter");
                var content = filter.data("griddly-filter-content");
                var displayEl = filter.data("griddly-filter-display");

                var dataType = filter.data("filter-datatype");
                var display = null;


                // TODO: shove formatted values back into boxes to ensure post is correct?
                // TODO: for numbers, do correctly shredded numeric (no symbols, but numbers and decimals etc.)
                // TODO: for dates, push actual formatted date
                if (filter.hasClass("griddly-filter-box"))
                {
                    if (!dontHide)
                        filter.find(".filter-trigger").popover("hide");

                    var val = trimToNull(content.find("input").first().val());

                    if (val != null)
                    {
                        if (dataType == "String")
                            display = 'Contains "' + getFormattedValue(val, dataType) + '"';
                        else
                            display = getFormattedValue(val, dataType);
                    }
                    else
                    {
                        display = "Any " + filter.data("filter-name");

                        content.find("input").first().val("");
                    }
                }
                else if (filter.hasClass("griddly-filter-range"))
                {
                    var val = trimToNull(content.find("input").first().val());
                    var valEnd = trimToNull(content.find("input").last().val());

                    if (val != null && valEnd != null)
                        display = getFormattedValue(val, dataType) + " - " + getFormattedValue(valEnd, dataType);
                    else if (val != null)
                        display = (dataType == "Date" ? "After " : ">= ") + getFormattedValue(val, dataType);
                    else if (valEnd != null)
                        display = (dataType == "Date" ? "Before " : "<= ") + getFormattedValue(valEnd, dataType);
                    else
                    {
                        display = "All " + filter.data("filter-name-plural");

                        content.find("input").first().val("");
                        content.find("input").last().val("");
                    }
                }
                else if (filter.hasClass("griddly-filter-list"))
                {
                    if (!filter.data("griddly-filter-ismultiple") && !dontHide)
                        filter.find(".filter-trigger").popover("hide");

                    var allItems = content.find("li:not(.griddly-list-group-header)");
                    var selectedItems = allItems.filter(".griddly-filter-selected");
                    var displayItemCount = parseInt(filter.data("griddly-filter-displayitemcount"));

                    if (selectedItems.length == allItems.length || (selectedItems.length == 0 && filter.data("griddly-filter-isnoneall")))
                        display = (allItems.length == 2 ? "Both " : "All ") + filter.data("filter-name-plural");
                    else if (selectedItems.length > displayItemCount)
                        display = selectedItems.length + " " + filter.data("filter-name-plural");
                    else if (selectedItems.length > 0 && selectedItems.length <= displayItemCount)
                    {
                        var itemTexts = selectedItems.find("a");
                        var display = $.trim($(itemTexts[0]).text());

                        for (var i = 1; i < selectedItems.length && i < displayItemCount; i++)
                            display += ", " + $.trim($(itemTexts[i]).text());
                    }
                    else
                        display = "No " + filter.data("filter-name-plural");
                }

                if (display)
                    displayEl.text(display);

                // TODO: remove below once M3 compat fixed
                // this.refresh(true);
            }, this));

            $(".griddly-filters-inline input, .griddly-filters-inline select", this.$element).on("change", $.proxy(function (event)
            {
                this.$element.trigger("filterchange.griddly", event.target);

                if (this.options.autoRefreshOnFilter)
                    this.refresh(true);
            }, this));

            $(".griddly-filters-inline .filter-content input", this.$element).keyup(function (event)
            {
                if (event.which == 13)
                {
                    $(this).blur();

                    var filter = $(this).data("griddly-filter");

                    filter.find(".filter-trigger").popover("hide");
                }
            });

            $(".griddly-filters-inline .filter-trigger", this.$element).each($.proxy(function (i, el)
            {
                var self = this;

                var filter = $(el).parents(".griddly-filter");
                var content = filter.find(".filter-content");

                filter.data("griddly-filter-content", content);
                filter.data("griddly-filter-display", filter.find(".griddly-filter-display"));
                filter.find("input").data("griddly-filter", filter);

                $(el).popover({
                    html: true,
                    placement: "bottom",
                    // TODO: figure out how to have griddly in modal and still use body container. as it is, something about the modal
                    // blocks inputs in popovers from getting focus. so as a fallback I put it back in the bouding container, 
                    // which will work but means it will get cut off if griddly is scrollable
                    container: this.$element.parents(".modal").length ? null : "body",
                    template: '<div class="popover griddly-filter-popover"><div class="arrow"></div><h3 class="popover-title"></h3><div class="popover-content"></div></div>',
                    content: function ()
                    {
                        return content;
                    }
                }).on("show.bs.popover", function ()
                {
                    self.$element.find(".griddly-filters-inline .filter-trigger").not(this).popover("hide");

                    content.find("input:first").select();

                    content.show();
                }).on("shown.bs.popover", function ()
                {
                    content.find("input:first").focus();
                }).on("hidden.bs.popover", function ()
                {
                    content.hide();
                });
            }, this));

            $(".griddly-filters-inline .filter-content  .dropdown-menu", this.$element).each($.proxy(function (i, el)
            {
                var self = this;

                $("a", el).click(function()
                {
                    var checkbox = $(this).find("input");
                    var item = $(this).parents("li");

                    var filter = checkbox.data("griddly-filter");

                    if (filter.data("griddly-filter-ismultiple"))
                    {
                        item.toggleClass("griddly-filter-selected");

                        checkbox.prop("checked", item.hasClass("griddly-filter-selected")).change();
                    }
                    else
                    {
                        var content = filter.data("griddly-filter-content");

                        content.find(".dropdown-menu li:not(.griddly-list-group-header)").not(item).removeClass("griddly-filter-selected");
                        content.find("input").not(checkbox).prop("checked", false);

                        item.addClass("griddly-filter-selected");
                        checkbox.prop("checked", true).change();
                    }
                });
            }, this));

            $(".griddly-filters-inline .filter-content .griddly-select-all", this.$element).each($.proxy(function (i, el)
            {
                var self = this;

                $(el).click(function ()
                {
                    $(this).parents(".filter-content").find(".dropdown-menu li:not(.griddly-list-group-header)").addClass("griddly-filter-selected");
                    $(this).parents(".filter-content").find("input").prop("checked", true).first().change();
                });
            }, this));

            $(".griddly-filters-inline .filter-content .griddly-clear", this.$element).each($.proxy(function (i, el)
            {
                var self = this;

                $(el).click(function ()
                {
                    $(this).parents(".filter-content").find(".dropdown-menu li:not(.griddly-list-group-header)").removeClass("griddly-filter-selected");
                    $(this).parents(".filter-content").find("input").prop("checked", false).first().change();
                });
            }, this));
        },

        exportFile: function(type, exec, data)
        {
            var params = this.buildRequest();
            
            params.exportFormat = type;

            if (exec)
            {
                $.extend(params, data);
                exec(this.options.url, params)
            }
            else
            {
                var url = this.options.url + (this.options.url.indexOf("?") == -1 ? "?" : "&") + $.param(params, true);
                window.location = url;
            }
        },

        getFilterValues: function()
        {
            var allFilters = $(".griddly-filters input, .griddly-filters select", this.$element).add(this.$inlineFilters);

            return serializeObject(allFilters);
        },

        setFilterValues: function(filters, isPatch)
        {
            this.options.autoRefreshOnFilter = false;

            if (isPatch !== true)
            {
                var allFilters = $(".griddly-filters input, .griddly-filters select", this.$element).add(this.$inlineFilters);

                allFilters.each(function ()
                {
                    $(this).val(filters[this.name]).change();
                });
            }
            else
            {
                for (var key in filters)
                    $("[name='" + key + "']").val(filters[key]).change();
            }

            this.options.autoRefreshOnFilter = true;
            this.refresh(true);
        },

        buildRequest: function(paging)
        {
            var postData = this.getFilterValues();

            if (this.options.sortFields.length)
            {
                for (var i = 0; i < this.options.sortFields.length; i++)
                {
                    var field = this.options.sortFields[i];

                    postData["sortFields[" + i + "][" + field.Field + "]"] = field.Direction;
                }
            }

            if (!paging)
            {
                $.extend(postData,
                {
                    pageNumber: this.options.pageNumber,
                    pageSize: this.options.pageSize
                });
            }

            return postData;
        },

        refresh: function(resetPage)
        {
            this.$element.trigger("beforerefresh.griddly");

            if (!this.options.url)
            {
                window.location = window.location;

                return;
            }

            if (resetPage)
                this.options.pageNumber = 0;

            this.options.lastSelectedRow = null;

            var postData = this.buildRequest();

            // TODO: cancel any outstanding calls

            $.ajax(this.options.url,
            {
                data: postData,
                traditional: true
            }).done($.proxy(function (data, status, xhr)
            {
                var count = parseInt(xhr.getResponseHeader("X-Griddly-Count"));
                var currentPageSize = parseInt(xhr.getResponseHeader("X-Griddly-CurrentPageSize"));

                this.options.count = count;
                this.options.pageCount = Math.ceil(this.options.count / this.options.pageSize);
                // TODO: handle smaller count
                
                var html = $(data);

                this.$element.find("tbody.data").replaceWith(html.filter("tbody"));

                var tfoot = this.$element.find("tfoot");

                if (tfoot.length && html.is("tfoot"))
                    tfoot.replaceWith(html.filter("tfoot"));

                var startRecord = this.options.pageNumber * this.options.pageSize;
                this.$element.find(".griddly-summary").html('<span class="hidden-xs">Records</span> ' + (startRecord + (this.options.count ? 1 : 0)) + ' <span class="hidden-xs">through</span><span class="visible-xs">-</span> ' + (startRecord + currentPageSize) + " of " + this.options.count);

                this.$element.find(".pageCount").html(this.options.pageCount);

                this.$element.find("input.pageNumber").val(this.options.pageNumber + 1);
                
                if (startRecord > this.options.count - this.options.pageSize)
                    this.$element.find(".next").hide();
                else
                    this.$element.find(".next").show();

                if (startRecord >= this.options.pageSize)
                    this.$element.find(".prev").show();
                else
                    this.$element.find(".prev").hide();

                if (this.options.count <= this.options.pageSize)
                    this.$element.find(".griddly-pager").hide();
                else
                    this.$element.find(".griddly-pager").show();
                
                //iterate through table and check rows that are in the selected list and have a checkbox
                var _this = this;
                $("tbody tr", this.$element).find("input[name=_rowselect]").each(function (index, e) {
                    var rowkey = $(e).data("rowkey");
                    if (_this.options.selectedRows[rowkey])
                        $(e).prop("checked", true);
                });

                this.$element.trigger("refresh.griddly",
                {
                    start: startRecord,
                    pageSize: currentPageSize,
                    count: count
                });
            }, this))
            .fail($.proxy(function (xhr, status, errorThrown)
            {
                if (this.options.onError)
                {
                    this.options.onError(xhr, status, errorThrown);
                }
                else
                {
                    var url = this.options.url + (this.options.url.indexOf('?') == -1 ? "?" : "&");

                    url += $.param(postData);

                    window.location = url;
                }

                this.$element.trigger("error.griddly");
            }, this));
        },

        getSelected: function(arrayIdNames)
        {
            if (arrayIdNames === "all")
                return this.options.selectedRows;

            if (!arrayIdNames)
                arrayIdNames = this.options.defaultRowIds;
            else if (typeof arrayIdNames === "string")
                arrayIdNames = [ arrayIdNames ];

            var result = {};
            for (var name in arrayIdNames)
                result[arrayIdNames[name]] = $.map(this.options.selectedRows, function (x) { return x[arrayIdNames[name]] });

            return result;
        },

        pageNumber: function(pageNumber)
        {
            if (pageNumber >= 0 && pageNumber < this.options.pageCount)
                this.options.pageNumber = pageNumber;
            // TODO: refresh auto?
        },

        pageSize: function(pageSize)
        {
            this.options.pageSize = pageSize;
            // TODO: refresh auto?
        },
        
        // destroy and unbind
        destroy: function ()
        {

        }
    };

    $.fn.griddly = function (option, parameter)
    {
        var value;
        var args = arguments;

        this.each(function ()
        {
            var data = $(this).data('griddly'),
                options = typeof option == 'object' && option;

            // initialize griddly
            if (!data)
            {
                var instanceOptions = $.extend({}, $.fn.griddly.defaults, options);

                $(this).data('griddly', (data = new Griddly(this, instanceOptions)));
            }

            // call griddly method
            if (typeof option == 'string')
            {
                value = data[option].apply(data, Array.prototype.slice.call(args, 1));
            }
        });

        if (value !== undefined)
            return value;
        else
            return this;
    };

    $.fn.griddly.defaults =
    {
        pageNumber: 0,
        pageSize: 20,
        onError: null,
        isMultiSort: true,
        lastSelectedRow: null,
        rowClickModal: null,
        selectedRows: null,
        autoRefreshOnFilter: true
    };

    function GriddlyButton()
    { }

    GriddlyButton.handleClick = $.proxy(function (event)
    {
        var button = $(event.currentTarget);
        var griddly = button.closest("[data-role=griddly]");
        var url = button.data("url");
        var toggle = button.data("toggle");
        var onclick = button.data("onclick");
        var confirmMessage = button.data("confirm-message");
        var enableOnSelection = button.data("enable-on-selection");
        var rowIds = button.data("rowids");

        if ((typeof confirmMessage === "undefined" || confirm(confirmMessage)))
        {
            if (button.triggerHandler("beforeExecute") !== false)
            {
                if (toggle && ["ajaxbulk", "postcriteria", "ajax", "post"].indexOf(toggle) > -1)
                {
                    if (!url)
                        url = button.attr("href");

                    var selection = {};
                    if (griddly)
                    {
                        selection = griddly.griddly("getSelected", rowIds);

                        if (selection.value)
                        {
                            selection.ids = selection.value;
                            delete selection.value;
                        }
                    }

                    switch (toggle)
                    {
                        case "ajaxbulk":
                            if (selection[Object.keys(selection)[0]].length == 0 && enableOnSelection)
                                return;

                            return this.ajaxBulk(url, selection, button, griddly);

                        case "post":
                            if (selection[Object.keys(selection)[0]].length == 0 && enableOnSelection)
                                return;

                            return this.post(url, selection, button, griddly);

                        case "postcriteria":
                            if (!griddly)
                                return;

                            return this.postCriteria(url, griddly.griddly("buildRequest"));

                        case "ajax":
                            if (selection[Object.keys(selection)[0]].length == 0 && enableOnSelection)
                                return;

                            return this.ajax(url, selection, button, griddly);
                    }
                }

                if (onclick)
                {
                    var f = window[onclick];

                    if ($.isFunction(f))
                    {
                        return f.call(button, rowIds);
                    }

                    throw "onclick must be a global function";
                    // we do not support eval cause it's insecure
                }

                return true;
            }
        }

        return false;
    }, GriddlyButton);

    GriddlyButton.ajaxBulk = function (url, selection, button, griddly)
    {
        $.ajax(url,
        {
            data: selection,
            traditional: true,
            type: "POST"
        }).done($.proxy(function (data, status, xhr)
        {
            // TODO: handle errors
            // TODO: go back to first page?
            griddly.griddly("refresh");

            button.triggerHandler("afterExecute", [data, status, xhr]);
        }, this));
    };

    GriddlyButton.post = function (url, selection, button, griddly)
    {
        var inputs = "";

        var token = $("input[name^=__RequestVerificationToken]").first();

        if (token.length)
            inputs += '<input type="hidden" name="' + token.attr("name") + '" value="' + token.val() + '" />';

        for (var idname in selection)
        {
            $.each(selection[idname], function () {
                inputs += "<input name=\"" + idname + "\" value=\"" + this + "\" />";
            });
        }

        $("<form action=\"" + url + "\" method=\"post\">" + inputs + "</form>")
            .appendTo("body").submit().remove();

        return false;
    };

    GriddlyButton.postCriteria = function (url, request, button, griddly)
    {
        var inputs = "";

        var token = $("input[name^=__RequestVerificationToken]").first();

        if (token.length)
            inputs += '<input type="hidden" name="' + token.attr("name") + '" value="' + token.val() + '" />';

        for (var key in request)
            inputs += '<input name="' + key + '" value="' + request[key] + '" />';

        $("<form action=\"" + url + "\" method=\"post\">" + inputs + "</form>")
            .appendTo("body").submit().remove();
    };

    GriddlyButton.ajax = function (url, selection, button, griddly)
    {
        for (var i = 0; i < selection[Object.keys(selection)[0]].length; i++)
        {
            var postdata = {};
            for (var k in selection)
            {
                postdata[k === "ids" ? "id" : k] = selection[k][i];
            }

            $.ajax(url,
            {
                data: postdata,
                type: "POST"
            }).done($.proxy(function (data, status, xhr)
            {
                // TODO: handle errors
                // TODO: go back to first page?
                griddly.griddly("refresh");

                button.triggerHandler("afterExecute", [data, status, xhr]);
            }, this));
        }
    };

    $(function()
    {
        $("[data-role=griddly]").griddly();
        $(document).on("click", "[data-role=griddly-button]", GriddlyButton.handleClick);

        // patch stupid bootstrap js so it doesn't .empty() our inline filter dropdowns
        // remove once bs fixes: https://github.com/twbs/bootstrap/pull/14244
        var setContent = $.fn.popover.Constructor.prototype.setContent;

        $.fn.popover.Constructor.prototype.setContent = function ()
        {
            var $tip = this.tip();

            $tip.find('.popover-content').children().detach();

            setContent.call(this);
        };
    });
}(window.jQuery);