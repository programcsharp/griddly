/*
 * Editly script
 * http://griddly.com
 * Copyright 2013-2015 Chris Hynes and Data Research Group, Inc.
 * Licensed under MIT (https://github.com/programcsharp/griddly/blob/master/LICENSE)
 *
 * WARNING: Don't edit this file -- it'll be overwitten when you upgrade.
 *
 */

!function ($)
{
    "use strict"; // jshint ;_;

    var Editly = function(element, options)
    {
        this.$element = $(element);
        this.options = options;
        this.create();
    };

    var ARROW_LEFT = 37, ARROW_UP = 38, ARROW_RIGHT = 39, ARROW_DOWN = 40, ENTER = 13, ESC = 27, TAB = 9;

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

    Editly.prototype = {
        constructor: Editly,

        // create and bind
        create: function ()
        {
            var self = this;
            var active = null;
            //var url = this.$element.data("griddly-url");

            //this.options.url = url;

            this.$element.find("tbody td").each(function ()
            {
                if (!this.tabIndex || this.tabIndex < 0)
                    this.tabIndex = 0;
            });

            var movement = function (element, keycode)
            {
                if (keycode === ARROW_RIGHT)
                    return element.next('td');
                else if (keycode === ARROW_LEFT)
                    return element.prev('td');
                else if (keycode === ARROW_UP)
                    return element.parent().prev().children().eq(element.index());
                else if (keycode === ARROW_DOWN)
                    return element.parent().next().children().eq(element.index());

                return [];
            };

            var showEditor = function (select)
            {
                active = self.$element.find('td:focus');

                if (active.length)
                {
                    var template = self.options.editors[active[0].cellIndex];
                    var editor = template.is(":input") ? template : template.find(":input");
                    
                    var validator = editor.parents("form").data('validator')

                    //validator.settings.showErrors = function (errorMap, errorList)
                    //{

                    //};

                    //validator.settings.errorPlacement = function (error, element)
                    //{
                    //    $(template).popover(
                    //    {
                    //        container: "body",
                    //        content: error.html(),
                    //        html: true
                    //    }).popover("show");
                    //};

                    template
                        .show()
                        .offset(active.offset())
                        .width(template == editor ? active.width() : active.outerWidth())
                        .height(template == editor ? active.height() : active.outerHeight());

                    editor.val(active.text())
                        .removeClass('error')
                        .css(active.css(self.options.cloneProperties))
                        .height(active.height())
                        .focus()
                        .off("blur")
                        .on("blur", function()
                        {
                            active.text(editor.val());
                            template.hide();
                            template.popover("hide");
                        })
                        .off("keydown")
                        .on("keydown", function (e)
                        {
                            if (e.which === ENTER)
                            {
                                //active.text(editor.val());
                                //setActiveText();

                                template.hide();
                                active.focus();

                                e.preventDefault();
                                e.stopPropagation();
                            }
                            else if (e.which === ESC)
                            {
                                editor.val(active.text());

                                e.preventDefault();
                                e.stopPropagation();

                                template.hide();
                                active.focus();
                            }
                            else if (e.which === TAB)
                            {
                                active.focus();

                                // Have to move here because dropdown eats it somehow if not
                                var possibleMove = movement(active, e.shiftKey ? ARROW_LEFT : ARROW_RIGHT);

                                if (possibleMove.length > 0)
                                {
                                    possibleMove.focus();

                                    e.preventDefault();
                                    e.stopPropagation();
                                }
                            }
                            else if (this.selectionEnd - this.selectionStart === this.value.length ||
                                (this.tagName == "SELECT" && (e.which == ARROW_RIGHT || e.which === ARROW_LEFT)))
                            {
                                var possibleMove = movement(active, e.which);

                                if (possibleMove.length > 0)
                                {
                                    possibleMove.focus();

                                    e.preventDefault();
                                    e.stopPropagation();
                                }
                            }
                        });

                    if (template == editor)
                        editor.width(active.width());

                    if (editor.valid())
                    {
                        validator.settings.unhighlight(editor);

                        // TODO: do we need this if app has proper bs highlight method?
                        editor.parents(".form-group").removeClass("has-error");
                    }
                    else
                    {
                        validator.settings.highlight(editor);

                        // TODO: do we need this if app has proper bs highlight method?
                        editor.parents(".form-group").addClass("has-error");

                        window.setTimeout(function ()
                        {
                            $(template)
                                .popover(
                                {
                                    container: "body",
                                    content: function () { return validator.errorList[0].message; }
                                })
                                .popover("show");
                        }, 100);
                    }

                    if (select)
                    {
                        editor.select();
                    }
                }
            };

            $(this.$element).on("click keypress dblclick", $.proxy(function (e)
            {
                showEditor(true);
            }, this));

            $(this.$element).on("keydown", $.proxy(function (e)
            {
                var prevent = true,
                    possibleMove = movement($(e.target), e.which);

                if (possibleMove.length > 0)
                {
                    possibleMove.focus();
                }
                else if (e.which === ENTER)
                {
                    showEditor(false);
                }
                else if (e.which === 17 || e.which === 91 || e.which === 93)
                {
                    showEditor(true);
                    prevent = false;
                }
                else
                {
                    prevent = false;
                }

                if (prevent)
                {
                    e.stopPropagation();
                    e.preventDefault();
                }
            }, this));
        },
        
        // destroy and unbind
        destroy: function ()
        {

        }
    };

    $.fn.editly = function (option, parameter)
    {
        var value;
        var args = arguments;

        this.each(function ()
        {
            var data = $(this).data('editly'),
                options = typeof option == 'object' && option;

            // initialize editly
            if (!data)
            {
                var instanceOptions = $.extend({}, $.fn.editly.defaults, options);

                $(this).data('editly', (data = new Editly(this, instanceOptions)));
            }

            // call editly method
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

    $.fn.editly.defaults =
    {
        cloneProperties: ['padding', 'padding-top', 'padding-bottom', 'padding-left', 'padding-right',
                  'text-align', 'font', 'font-size', 'font-family', 'font-weight']//,
                  //'border', 'border-top', 'border-bottom', 'border-left', 'border-right']

    };

    $(function()
    {
        $("[data-role=editly]").editly();
    });
}(window.jQuery);