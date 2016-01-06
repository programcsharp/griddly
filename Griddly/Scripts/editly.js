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
            this.bind();
        },
        
        // destroy and unbind
        destroy: function ()
        {
            this.unbind();
        },

        unbind: function()
        {
            var self = this;
            this.$element.unbind(".editly");
            this.$element.find("tbody td").each(function () {
                if (self.options.editors[this.cellIndex] != null) {
                    $(this).removeAttr("tabindex");
                    $(this).removeClass("editly-editable");
                }
                else
                    $(this).css("cursor", "");
                
            });
            this.$element.find("tbody tr").each(function () {
                $(this).data("griddly-url", $(this).data("griddly-url-inactive"));
                $(this).data("griddly-url-inactive", "");
            });

            for (var i = 0; i < this.options.editors.length; i++) {
                $(this.options.editors[i]).hide();
                $(this.options.editors[i]).popover("hide");
            }
        },

        bind: function () {
            var self = this;
            var active = null;
            var template = null;
            var editor = null;
            var validator = null;

            var movement = function (element, keycode) {
                if (keycode === ARROW_RIGHT)
                    return element.nextAll('td[tabindex]:first');
                else if (keycode === ARROW_LEFT)
                    return element.prevAll('td[tabindex]:first');
                else if (keycode === ARROW_UP)
                    return element.parent().prev().children().eq(element.index());
                else if (keycode === ARROW_DOWN)
                    return element.parent().next().children().eq(element.index());

                return [];
            };

            var doValidation = function () {
                if (validator) {
                    var isValid = editor.valid();

                    if (isValid) {
                        hideValidation();
                    }
                    else if (validator) {
                        validator.settings.highlight(editor);

                        // TODO: do we need this if app has proper bs highlight method?
                        editor.parents(".form-group").addClass("has-error");

                        window.setTimeout(function () {
                            var message = validator.errorList[0].message;
                            $(template)
                                .popover(
                                {
                                    container: "body",
                                    content: message
                                })
                                .popover("show");
                        }, 100);
                    }
                    return isValid;
                }
                else {
                    return true;
                }
            };
            var hideValidation = function () {
                if (validator) {
                    validator.settings.unhighlight(editor);

                    // TODO: do we need this if app has proper bs highlight method?
                    editor.parents(".form-group").removeClass("has-error");

                    template.popover("hide");
                }
            };

            var hideEditor = function () {
                template.hide();
                hideValidation();
            };

            var saveActive = function () {
                var oldValue = active.data("value");
                if (oldValue === undefined) oldValue = self.options.parseText(active.text(), template);
                var oldText = active.text();
                var newValue = editor.val();
                
                if (oldValue != newValue) {
                    var newText = editor[0].tagName=="SELECT"?(editor.find("option:selected").text()):editor.val();
                    active.text(self.options.formatText(newText, template));
                    active.data("value", newValue);
                    self.options.save(
                    {
                        id: active.parents("tr:first").data("id"),
                        field: $(active.parents("table:first").find("col")[active[0].cellIndex]).data("field"),
                        value: newValue,
                        oldValue: oldValue,
                        oldText: oldText,
                        cell: active
                    });
                }
            };

            var showEditor = function (select) {
                var last = active;
                var newActive = self.$element.find('td:focus');

                if (newActive.length && self.options.editors[newActive[0].cellIndex]) {
                    active = newActive;
                    template = self.options.editors[active[0].cellIndex];
                    editor = template.is(":input") ? template : template.find(":input");
                    validator = editor.parents("form").data('validator');

                    var handleBlur = function () {
                        // TODO: is there a more efficient way for this?
                        if (!$(document.activeElement).parents().is(template)) {
                            if (!validator || editor.valid()) {
                                saveActive();

                                hideEditor();
                            }
                            else {
                                editor.focus();

                                doValidation();
                            }
                        }
                    };

                    if (last != null && last.length > 0) {
                        var lastTemplate = self.options.editors[last[0].cellIndex];

                        if (lastTemplate != template) {
                            lastTemplate.hide();
                        }
                    }

                    template
                        .show()
                        .offset(active.offset())
                        .width(template == editor ? active.width() : active.outerWidth())
                        .height(template == editor ? active.height() : active.outerHeight())
                        .off("blur change");

                    editor.on("blur change", handleBlur);

                    var value = active.data("value");
                    if (value === undefined)
                        value = self.options.parseText(active.text(), template);

                    editor.val(value)
                        .removeClass('error')
                        .css(active.css(self.options.cloneProperties))
                        .height(active.height())
                        .focus()
                        .off("keydown")
                        .on("keydown", function (e) {
                            if (e.which === ENTER) {
                                if (doValidation()) {
                                    saveActive();
                                    hideEditor();
                                    active.focus();
                                    
                                    var possibleMove = movement(active, e.shiftKey ? ARROW_UP : ARROW_DOWN);

                                    if (possibleMove.length > 0) {
                                        possibleMove.focus();

                                        e.preventDefault();
                                        e.stopPropagation();
                                    }
                                }
                            }
                            else if (e.which === ESC) {
                                //Reset to original value
                                var value = active.data("value");
                                if (value === undefined)
                                    value = self.options.parseText(active.text(), template);
                                editor.val(value);

                                hideEditor();
                                active.focus();

                                e.preventDefault();
                                e.stopPropagation();
                            }
                            else if (e.which === TAB) {
                                if (doValidation()) {
                                    saveActive();
                                    hideEditor();
                                    active.focus();

                                    // Have to move here because dropdown eats it somehow if not
                                    var possibleMove = movement(active, e.shiftKey ? ARROW_LEFT : ARROW_RIGHT);
                                    if (possibleMove.length > 0) {

                                        possibleMove.focus();

                                        e.preventDefault();
                                        e.stopPropagation();
                                    }
                                }
                            }
                            else if (this.tagName == "INPUT" && this.selectionEnd - this.selectionStart === this.value.length ||
                                (this.tagName == "SELECT" && (e.which == ARROW_RIGHT || e.which === ARROW_LEFT)) ||
                                (this.tagName == "INPUT" && (e.which == ARROW_UP || e.which === ARROW_DOWN))
                            ) {
                                var possibleMove = movement(active, e.which);

                                if (possibleMove.length > 0) {
                                    if (doValidation()) {
                                        saveActive();
                                        hideEditor();
                                        possibleMove.focus();

                                        e.preventDefault();
                                        e.stopPropagation();
                                    }
                                }
                            }
                        });
    
                    if (template == editor)
                        editor.width(active.width());

                    if (select)
                        editor.select();
                }
            };

            this.$element.find("tbody td").each(function () {
                if (self.options.editors[this.cellIndex] != null) {
                    if (!this.tabIndex || this.tabIndex < 0) {
                        this.tabIndex = 0;
                        $(this).addClass("editly-editable");
                    }
                }
                else
                    $(this).css("cursor", "default");
            });
            this.$element.find("tbody tr").each(function () {
                $(this).data("griddly-url-inactive", $(this).data("griddly-url"));
                $(this).data("griddly-url", "");
            });

            $(this.$element).on("click.editly keypress.editly dblclick.editly", $.proxy(function (e) {
                if (doValidation()) {
                    if(active&&active.length>0)
                        saveActive();
                    showEditor(true);
                } else {
                    e.stopPropagation();
                    e.preventDefault();
                }
            }, this));

            $(this.$element).on("keydown.editly", $.proxy(function (e) {
                var prevent = true,
                    possibleMove = movement($(e.target), e.which);

                if (possibleMove.length > 0) {
                    possibleMove.focus();
                }
                else if (e.which === ENTER) {
                    if (active != null && active.length > 0 && active[0].tagName == "INPUT") {
                        possibleMove = movement($(e.target), e.shiftKey ? ARROW_UP : ARROW_DOWN)
                        if (possibleMove.length > 0) {
                            possibleMove.focus();
                        }
                    }
                    else {
                        showEditor(false);
                    }
                }
                else if (e.which === 113) {
                    showEditor(false);
                }
                else if (e.which === 17 || e.which === 91 || e.which === 93) {
                    showEditor(true);
                    prevent = false;
                }
                else if (e.which === 32) {
                    showEditor(true);
                    prevent = true;
                }
                else {
                    prevent = false;
                }

                if (prevent) {
                    e.stopPropagation();
                    e.preventDefault();
                }
            }, this));
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
        save: function () { },
        parseText: function (val, editor) { return val; },
        formatText: function (val, editor) { return val; },
        cloneProperties: ['padding', 'padding-top', 'padding-bottom', 'padding-left', 'padding-right',
                  'text-align', 'font', 'font-size', 'font-family', 'font-weight']//,
        //'border', 'border-top', 'border-bottom', 'border-left', 'border-right']
    };

    $(function()
    {
        $("[data-role=editly]").editly();
    });
}(window.jQuery);