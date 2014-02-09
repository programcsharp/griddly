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
            var sortDefaults = this.$element.data("griddly-sortdefaults");
            var isMultiSort = this.$element.data("griddly-multisort");
            var onRefresh = this.$element.data("griddly-onrefresh");
            var rowClickModal = this.$element.data("griddly-rowclickmodal");

            this.options.url = url;
            this.options.count = parseInt(count);
            if (pageSize)
                this.options.pageSize = parseInt(pageSize);
            this.options.pageCount = this.options.count * this.options.pageSize;
            this.options.rowClickModal = rowClickModal;

            if (isMultiSort != null)
                this.options.isMultiSort = isMultiSort == true;

            this.options.sortFields = { };

            if (sortDefaults)
            {
                for (var i = 0; i < sortDefaults.length; i++)
                {
                    var sort = sortDefaults[i].split(" ");
                    var direction = "ASC";

                    if (sort.length == 2 && sort[1].toLowerCase() == "desc")
                        direction = "DESC";

                    this.options.sortFields[sort[0]] = direction;
                }
            }

            if (onRefresh && Object.prototype.toString.call(window[onRefresh]) == '[object Function]')
                this.options.onRefresh = window[onRefresh];

            $("form", this.$element).attr("onsubmit", "return false;");

            $("a.next", this.$element).on("click", $.proxy(function (event)
            {
                this.options.pageNumber++;

                this.refresh();

                event.preventDefault();
            }, this));

            $("a.prev", this.$element).on("click", $.proxy(function (event)
            {
                this.options.pageNumber--;

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
                this.$element.find("form")[0].reset();

                this.refresh(true);
            }, this));

            $(".filters.inline input, .filters.inline select", this.$element).on("change", $.proxy(function (event)
            {
                this.refresh(true);
            }, this));

            var onRowClick = $.proxy(function (e)
            {
                var url = $(e.target).parents("tr").data("griddly-url");

                if (url)
                {
                    if (this.options.rowClickModal)
                    {
                        $(this.options.rowClickModal).removeData("modal").modal({ remote: url });
                    }
                    else
                    {
                        if (e.which == 2 || e.ctrlKey)
                            window.open(url);
                        else
                            window.location = url;
                    }
                }
            }, this);

            $(this.$element).on("click", "tbody.data tr td:not(:has(input))", onRowClick);
            $(this.$element).on("mouseup", "tbody.data tr td:not(:has(input))", onRowClick);

            $(this.$element).on("click", "thead tr.columnHeaders th", $.proxy(function (event)
            {
                var sortField = $(event.currentTarget).data("griddly-sortfield");

                if (sortField)
                {
                    var newSort = (this.options.sortFields[sortField] || "DESC") == "DESC" ? "ASC" : "DESC";

                    this.options.sortFields[sortField] = newSort;

                    if (newSort == "ASC")
                        $(event.currentTarget).removeClass("sorted_d").addClass("sorted_a");
                    else
                        $(event.currentTarget).removeClass("sorted_a").addClass("sorted_d");

                    if (!this.options.isMultiSort || !event.ctrlKey)
                    {
                        for (var x in this.options.sortFields)
                        {
                            if (x != sortField)
                            {
                                $(event.currentTarget).parents("tr").find("th[data-griddly-sortfield='" + x + "']").removeClass("sorted_a").removeClass("sorted_d");

                                delete this.options.sortFields[x];
                            }
                        }
                    }

                    this.refresh(true);
                }
            }, this));

            $(this.$element).on("change", "input[name=_rowselect]", $.proxy(function (event)
            {
                var op = this.$element.find("input[name=_rowselect]:checked").length ? "remove" : "add";

                $(this.$element).find("[data-enable-on-selection=true]")[op + "Class"]("disabled");

            }, this));

            $(this.$element).on("click", "td.griddly-select", $.proxy(function (event)
            {
                var $target = $(event.target);

                if (!$target.is("input"))
                {
                    var $checkbox = $target.find("input[name=_rowselect]");

                    $checkbox.prop("checked", !$checkbox.prop("checked"));
                }

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
                    this.$element.find("input[name=_rowselect]").prop("checked", false);
                else
                    this.$element.find("input[name=_rowselect]").prop("checked", true);
            }, this));

            $(this.$element).on("click", "[data-toggle=post]", $.proxy(function (event)
            {
                var url = $(event.currentTarget).data("url");
                var ids = this.getSelected();
                var inputs = "";

                if (ids.length == 0)
                    return;

                $.each(ids, function ()
                {
                    inputs += "<input name=\"ids\" value=\"" + this + "\" />";
                });

                $("<form action=\"" + url + "\" method=\"post\">" + inputs + "</form>")
                    .appendTo("body").submit().remove();
            }, this));

            $(this.$element).on("click", "[data-toggle=ajaxbulk]", $.proxy(function (event)
            {
                var url = $(event.currentTarget).data("url");
                var ids = this.getSelected();

                if (ids.length == 0)
                    return;

                $.ajax(url,
                {
                    data: { ids : ids },
                    traditional: true,
                    type: "POST"
                }).done($.proxy(function (data, status, xhr)
                {
                    // TODO: handle errors
                    // TODO: go back to first page?
                    this.refresh();
                }, this));
            }, this));

            $(this.$element).on("click", "[data-toggle=ajax]", $.proxy(function (event)
            {
                var url = $(event.currentTarget).data("url");
                var ids = this.getSelected();

                if (ids.length == 0)
                    return;

                for (var i = 0; i < ids.length; i++)
                {
                    $.ajax(url,
                    {
                        data: { id: ids[i] },
                        type: "POST"
                    }).done($.proxy(function (data, status, xhr)
                    {
                        // TODO: handle errors
                        // TODO: go back to first page?
                        this.refresh();
                    }, this));
                }
            }, this));
        },

        exportFile: function(type)
        {
            var params = this.buildRequest();
            
            params.exportFormat = type;

            var url = this.options.url + (this.options.url.indexOf("?") == -1 ? "?" : "&") + $.param(params, true);

            window.location = url;
        },

        buildRequest: function()
        {
            var postData = serializeObject($(".filters input[type=text], .filters select", this.$element));

            if (this.options.sortFields)
            {
                var sortFields = [];

                for (var x in this.options.sortFields)
                    sortFields.push(x + " " + this.options.sortFields[x]);

                if (sortFields.length)
                    postData.sortFields = sortFields.join(",");
            }

            $.extend(postData,
            {
                pageNumber: this.options.pageNumber,
                pageSize: this.options.pageSize
            });

            return postData;
        },

        setParams: function (data)
        {
            // TODO: route normal filter changes through here?

            $(".filters input[type=text]", this.$element).val("");
            $(".filters select option", this.$element).prop("selected", false);

            for (var key in data)
            {
                var $input = $(".filters [name='" + key + "']", this.$element);

                if ($input.is("select"))
                {
                    var values = data[key];

                    if (Object.prototype.toString.call(values) !== "[object Array]")
                        values = values.split(",");

                    for (var i = 0; i < values.length; i++)
                        $("option[value='" + values[i] + "']", $input).prop("selected", true);
                }
                else
                    $input.val(data[key]);
            }

            if (data.sortFields)
            {
                this.options.sortFields = { };
                $("thead th[data-griddly-sortfield]", this.$element).removeClass("sorted_a sorted_d");

                var sortFields = data.sortFields.split(",");

                for (var i = 0; i < sortFields.length; i++)
                {
                    var values = sortFields[i].split(" ");

                    this.options.sortFields[values[0]] = values[1];

                    $("thead th[data-griddly-sortfield='" + values[0] + "']", this.$element).addClass("sorted_" + values[1] == "ASC" ? "a" : "d");
                }
            }

            if (data.pageNumber)
                this.options.pageNumber = data.pageNumber;
            if (data.pageSize)
                this.options.pageSize = data.pageSize;

            this.refresh();
        },

        refresh: function(resetPage)
        {
            if (resetPage)
                this.options.pageNumber = 0;

            this.options.lastSelectedRow = null;

            var postData = this.buildRequest();

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
                this.$element.find("tbody.data").replaceWith(data);

                var startRecord = this.options.pageNumber * this.options.pageSize;

                this.$element.find(".griddly-summary").html("Records " + (startRecord + (this.options.count ? 1 : 0)) + " through " + (startRecord + currentPageSize) + " of " + this.options.count);

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

                $(this.$element).find("[data-enable-on-selection=true]").addClass("disabled");

                if (this.options.onRefresh)
                    this.options.onRefresh(this, startRecord, currentPageSize, count, postData);
				// TODO: should we remove the onClientRefresh method?
                this.$element.trigger("refresh", 
                {
                    start: startRecord,
                    pageSize: currentPageSize,
                    count: count
                });
            }, this));
        },

        getSelected: function()
        {
            var data = serializeObject(this.$element.find("input[name=_rowselect]"))._rowselect;

            if (!data)
                data = [];
            else if (!data.push)
                data = [data];

            return data;
        },

        onRefresh: function(onRefresh)
        {
            this.options.onRefresh = onRefresh;
        },

        pageNumber: function(pageNumber)
        {
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
                value = data[option](parameter);
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
        onRefresh: null,
        isMultiSort: true,
        lastSelectedRow: null,
        rowClickModal: null
    };

    $(function()
    {
        $("[data-role=griddly]").griddly();
    });
}(window.jQuery);
