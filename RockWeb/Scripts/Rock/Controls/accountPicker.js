(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.accountPicker = (function () {
        var AccountPicker = function (options) {
            this.options = options;
            // set a flag so that the picker only auto-scrolls to a selected item once. This prevents it from scrolling at unwanted times
            this.alreadyScrolledToSelected = false;
            this.iScroll = null;
        },
            exports;

        AccountPicker.prototype = {
            constructor: AccountPicker,
            initialize: function () {
                var $control = $('#' + this.options.controlId),
                    $tree = $control.find('.treeview'),
                    treeOptions = {
                        customDataItems: this.options.customDataItems,
                        displayChildItemCountLabel: this.options.displayChildItemCountLabel,
                        enhanceForLongLists: this.options.enhanceForLongLists,
                        multiselect: this.options.allowMultiSelect,
                        categorySelection: this.options.allowCategorySelection,
                        categoryPrefix: this.options.categoryPrefix,
                        restUrl: this.options.restUrl,
                        searchRestUrl: this.options.searchRestUrl,
                        restParams: this.options.restParams,
                        expandedIds: this.options.expandedIds,
                        expandedCategoryIds: this.options.expandedCategoryIds,
                        showSelectChildren: this.options.showSelectChildren,
                        id: this.options.startingId
                    },

                    // used server-side on postback to display the selected nodes
                    $hfItemIds = $control.find('.js-item-id-value'),
                    // used server-side on postback to display the expanded nodes
                    $hfExpandedIds = $control.find('.js-initial-item-parent-ids-value'),
                    $hfExpandedCategoryIds = $control.find('.js-expanded-category-ids');

                // Custom mapping override to add features items
                this.options.mapItems = function (arr, treeView) {

                    return $.map(arr, function (item) {

                        var node = {
                            id: item.Guid || item.Id,
                            name: item.Name || item.Title,
                            iconCssClass: item.IconCssClass,
                            parentId: item.ParentId,
                            hasChildren: item.HasChildren,
                            isActive: item.IsActive,
                            countInfo: item.CountInfo,
                            isCategory: item.IsCategory
                        };

                        // Custom node properties passed in from the *Picker.cs using the ItemPicker base CustomDataItems property
                        if (treeView.options.customDataItems && treeView.options.customDataItems.length > 0) {
                            treeView.options.customDataItems.forEach(function (dataItem, idx) {
                                if (!node.hasOwnProperty(dataItem)) {
                                    node['' + dataItem.itemKey + ''] = item['' + dataItem.itemValueKey + ''];
                                }
                            });
                        }

                        if (node.parentId === null) {
                            node.parentId = '0';
                        }

                        if (item.Children && typeof item.Children.length === 'number') {

                            // traverse using the _mapArrayDefault in rockTree.js
                            node.children = _mapArrayDefault(item.Children, treeView);
                        }

                        if (node.isCategory) {
                            node.id = treeView.options.categoryPrefix + node.id;
                        }

                        return node;
                    });
                }

                if (typeof this.options.mapItems === 'function') {
                    treeOptions.mapping = {
                        mapData: this.options.mapItems
                    };
                }

                // clean up the tree (in case it was initialized already, but we are rebuilding it)
                var rockTree = $tree.data('rockTree');

                if (rockTree) {
                    rockTree.nodes = [];
                }
                $tree.empty();

                var $scrollContainer = $control.find('.scroll-container .viewport');
                var $scrollIndicator = $control.find('.track');
                this.iScroll = new IScroll($scrollContainer[0], {
                    mouseWheel: true,
                    indicators: {
                        el: $scrollIndicator[0],
                        interactive: true,
                        resize: false,
                        listenY: true,
                        listenX: false,
                    },
                    click: false,
                    preventDefaultException: { tagName: /.*/ }
                });

                // Since some handlers are "live" events, they need to be bound before tree is initialized
                this.initializeEventHandlers();

                if ($hfItemIds.val() && $hfItemIds.val() !== '0') {
                    treeOptions.selectedIds = $hfItemIds.val().split(',');
                }

                if ($hfExpandedIds.val()) {
                    treeOptions.expandedIds = $hfExpandedIds.val().split(',');
                }

                if ($hfExpandedCategoryIds.val()) {
                    treeOptions.expandedCategoryIds = $hfExpandedCategoryIds.val().split(',');
                }

                // Initialize the rockTree and pass the tree options also makes http fetches
                $tree.rockTree(treeOptions);

                this.updateScrollbar();
            },
            initializeEventHandlers: function () {
                var self = this,
                    $control = $('#' + this.options.controlId),
                    $spanNames = $control.find('.selected-names'),
                    $hfItemIds = $control.find('.js-item-id-value'),
                    $hfItemNames = $control.find('.js-item-name-value');

                // Bind tree events
                $control.find('.treeview')
                    .on('rockTree:selected', function (e) {

                    })
                    .on('rockTree:itemClicked', function (e, data) {
                        // make sure it doesn't auto-scroll after something has been manually clicked
                        self.alreadyScrolledToSelected = true;
                        if (!self.options.allowMultiSelect) {
                            $control.find('.picker-btn').trigger('click');
                        }
                    })
                    .on('rockTree:expand rockTree:collapse rockTree:dataBound', function (evt, data) {

                        self.updateScrollbar();
                    })
                    .on('rockTree:rendered', function (evt, data) {

                        self.createSearchControl();
                        self.scrollToSelectedItem();
                    })
                    .on('rockTree:fetchCompleted', function (evt, data) {
                        // intentionally empty
                    });

                $control.find('a.picker-label').on('click', function (e) {
                    e.preventDefault();
                    $(this).toggleClass("active");
                    $control.find('.picker-menu').first().toggle(0, function () {
                        self.scrollToSelectedItem();
                    });
                });

                $control.find('.picker-cancel').on('click', function () {
                    $(this).toggleClass("active");
                    $(this).closest('.picker-menu').toggle(0, function () {
                        self.updateScrollbar();
                    });
                    $(this).closest('a.picker-label').toggleClass("active");

                });

                // Preview Selection link click
                $control.find('.picker-preview').on('click', function () {

                    if (previewData) {
                        var listHtml = '';
                        previewData.forEach(function (node) {

                            listHtml +=
                                '<div id="preview-item-' + node.nodeId + '" class="container-fluid">' +
                                '      <div class="row">' +
                                '        <div class="col-xs-10">' +
                                '             <li class="rocktree-item rocktree-folder rocktree-preview-item">' +
                                '            <span class="rocktree-name" title="' + node.title + '">' +
                                '              <h5><span class="rocktree-node-name-text text-color">' + node.name + '</span><br/>' +
                                '              <span class="text-muted"><small>' + node.path.replaceAll('^', '<i class="fa fa-chevron-right pl-1 pr-1" aria-hidden="true"></i>') + '</small></span></h5>' +
                                '            </span>' +
                                '         </li>' +
                                '       </div>' +
                                '       <div class="col-xs-2 pt-2">' +
                                '         <a id="lnk-remove-preview-' + node.nodeId + '" title="Remove From Preview" class="btn btn-link text-muted"> <i class="fa fa-times"></i></a>' +
                                '       </div>' +
                                ' </div>' +
                                '</div>';
                        });

                        // Display preview list
                        var listHtmlView = '<ul class="rocktree">' +
                            listHtml +
                            '</ul>';

                        $('#' + self.options.controlId + '_treeItems').html(listHtmlView);
                        // Wire up remove event and remove from dataset
                        previewData.forEach(function (node) {

                            $('#lnk-remove-preview-' + node.nodeId).on('click', function (e) {

                                $('#preview-item-' + node.nodeId).remove();

                                self.removeSelectedNodes(node.nodeId);

                                previewData = previewData.filter(function (fNode) {
                                    return fNode.nodeId !== node.nodeId;
                                });

                                // Re-select the items on the tree if it have removed them all
                                if (!previewData || previewData.length === 0) {
                                    self.showTreeState();
                                    $control.find('.item-picker-search').show();
                                    self.rebuildTreeSelections();
                                }
                            });
                        });
                    }
                });

                // Tree View link click
                $control.find('.picker-treeview').on('click', function () {
                });

                // have the X appear on hover if something is selected
                if ($hfItemIds.val() && $hfItemIds.val() !== '0') {
                    $control.find('.picker-select-none').addClass('rollover-item');
                    $control.find('.picker-select-none').show();
                }

                // [Select] button click
                $control.find('.picker-btn').on('click', function (el) {

                    var rockTree = $control.find('.treeview').data('rockTree'),
                        selectedNodes = rockTree.selectedNodes,
                        selectedIds = [],
                        selectedNames = [];

                    $.each(selectedNodes, function (index, node) {
                        var nodeName = $("<textarea/>").html(node.name).text();
                        selectedNames.push(nodeName);
                        if (!selectedIds.includes(node.id)) {
                            selectedIds.push(node.id);
                        }
                    });

                    $hfItemIds.val(selectedIds.join(',')).trigger('change'); // .trigger('change') is used to cause jQuery to fire any "onchange" event handlers for this hidden field.
                    $hfItemNames.val(selectedNames.join(','));

                    // have the X appear on hover. something is selected
                    $control.find('.picker-select-none').addClass('rollover-item');
                    $control.find('.picker-select-none').show();

                    $spanNames.text(selectedNames.join(', '));
                    $spanNames.attr('title', $spanNames.text());

                    $(this).closest('a.picker-label').toggleClass("active");
                    $(this).closest('.picker-menu').toggle(0, function () {
                        self.updateScrollbar();
                    });

                    if (!(el && el.originalEvent && el.originalEvent.srcElement === this)) {
                        // if this event was called by something other than the button itself, make sure the execute the href (which is probably javascript)
                        var jsPostback = $(this).attr('href');
                        if (jsPostback) {
                            window.location = jsPostback;
                        }
                    }
                });

                $control.find('.picker-select-none').on("click", function (e) {
                    e.stopImmediatePropagation();
                    var rockTree = $control.find('.treeview').data('rockTree');
                    rockTree.clear();
                    $hfItemIds.val('0').trigger('change'); // .trigger('change') is used to cause jQuery to fire any "onchange" event handlers for this hidden field.
                    $hfItemNames.val('');

                    // don't have the X appear on hover. nothing is selected
                    $control.find('.picker-select-none').removeClass('rollover-item');
                    $control.find('.picker-select-none').hide();

                    $control.siblings('.js-hide-on-select-none').hide();

                    $spanNames.text(self.options.defaultText);
                    $spanNames.attr('title', $spanNames.text());
                });

                // clicking on the 'select all' btn
                $control.on('click', '.js-select-all', function (e) {
                    var rockTree = $control.find('.treeview').data('rockTree');

                    e.preventDefault();
                    e.stopPropagation();

                    var $itemNameNodes = rockTree.$el.find('.rocktree-name');

                    var allItemNodesAlreadySelected = true;
                    $itemNameNodes.each(function (a) {
                        if (!$(this).hasClass('selected')) {
                            allItemNodesAlreadySelected = false;
                        }
                    });

                    if (!allItemNodesAlreadySelected) {
                        // mark them all as unselected (just in case some are selected already), then click them to select them
                        $itemNameNodes.removeClass('selected');
                        $itemNameNodes.trigger('click');
                    } else {
                        // if all were already selected, toggle them to unselected
                        rockTree.setSelected([]);
                        $itemNameNodes.removeClass('selected');
                    }
                });
            },
            updateScrollbar: function (sPosition) {
                var self = this;
                // first, update this control's scrollbar, then the modal's
                var $container = $('#' + this.options.controlId).find('.scroll-container');

                if ($container.is(':visible')) {
                    if (!sPosition) {
                        sPosition = 'relative'
                    }
                    if (self.iScroll) {
                        self.iScroll.refresh();
                    }
                }

                // update the outer modal
                Rock.dialogs.updateModalScrollBar(this.options.controlId);
            },
            scrollToSelectedItem: function () {
                var $selectedItem = $('#' + this.options.controlId + ' [class^="picker-menu"]').find('.selected').first();
                if ($selectedItem.length && (!this.alreadyScrolledToSelected)) {
                    this.updateScrollbar();
                    this.iScroll.scrollToElement('.selected', '0s');
                    this.alreadyScrolledToSelected = true;
                } else {
                    // initialize/update the scrollbar
                    this.updateScrollbar();
                }
            },
            createSearchControl: function () {
                var self = this;
                var controlId = self.options.controlId;

                var $control = $('#' + controlId);

                var rockTree = $control.find('.treeview').data('rockTree');

                if (self.options.enhanceForLongLists === true) {

                    // A hidden value to store the current search criteria to be read on post-backs
                    var $searchValueField = $control.find('.js-existing-search-value');

                    var $searchControl =
                        $('<div class="rocktree-drawer form-group js-group-search" style="display: none;">' +
                            '	<span class="control-label d-none">Search</span>' +
                            '	<div id="pnlSearch_' + controlId + '" class= "input-group" > ' +
                            '		<input id="tbSearch_' + controlId + '" type="text" placeholder="Quick Find" class="form-control input-sm" />' +
                            '		<span class="input-group-btn">' +
                            '			<a id="btnSearch_' + controlId + '" class="btn btn-default btn-sm"><i class="fa fa-search"></i></a>' +
                            '		</span>' +
                            '	</div>' +
                            '</div>');

                    // Get all of the rendered items
                    var itemsHtml = $('#' + controlId + ' .treeview-items').html();
                    console.debug($('#treeviewItems_' + controlId).html());
                    // Add the search control after rendering
                    $('#treeview-view-port_' + controlId).prepend($searchControl.html());

                    var $searchInputControl = $('#tbSearch_' + controlId);

                    $('#btnSearch_' + controlId).off('click').on('click', function () {

                        var searchKeyword = $searchInputControl.val();
                        if (searchKeyword && searchKeyword.length > 0) {
                            console.debug(searchKeyword);
                            var searchRestUrl = self.options.searchRestUrl;
                            var restUrlParams = self.options.restParams + '/' + searchKeyword;

                            searchRestUrl += restUrlParams;

                            $.getJSON(searchRestUrl, function (data, status) {

                                console.debug(data);

                                if (data && status === 'success') {
                                    $('#<%=pnlTreeviewContent.ClientID%>').html('');
                                }
                                else {
                                    $('#<%=divSearchResults.ClientID%>').hide();
                                    $('#<%=divTreeView.ClientID%>').show();
                                    return;
                                }

                                var nodes = [];
                                for (var i = 0; i < data.length; i++) {
                                    var obj = data[i];
                                    var node = {
                                        nodeId: obj.Id,
                                        parentId: obj.ParentId,
                                        glcode: obj.GlCode,
                                        title: obj.Name + (obj.GlCode ? ' (' + obj.GlCode + ')' : ''),
                                        name: obj.Name,
                                        hasChildren: obj.HasChildren,
                                        isActive: obj.IsActive,
                                        path: obj.Path
                                    };

                                    nodes.push(node);
                                }

                                if (nodes) {
                                    var listHtml = '';
                                    nodes.forEach(function (v, idx) {
                                        listHtml +=
                                            '<div id="divSearchItem" class="container-fluid">' +
                                            '      <div class="row">' +
                                            '        <div class="col-xs-12 p-0">' +
                                            '             <li class="rocktree-item rocktree-folder rocktree-search-result-item">' +
                                            '         <a class="search-result-link" data-id="' + v.nodeId + '" href="javascript:void(0);">' +
                                            '            <span class="rocktree-name">' +
                                            '              <h5><span class="rocktree-node-name-text text-color">' + v.title + '</span><br/>' +
                                            '              <span class="text-muted"><small>' + v.path.replaceAll('^', '<i class="fa fa-chevron-right pl-1 pr-1" aria-hidden="true"></i>') + '</small></span></h5>' +
                                            '            </span></a>' +
                                            '         </li>' +
                                            '       </div>' +
                                            ' </div>' +
                                            '</div>';
                                    });

                                    $('#<%=divSearchResults.ClientID%>').html('<ul class="list-unstyled">' +
                                        listHtml +
                                        '</ul>');
                                }
                            });
                        }

                    });

                    // Handle the input searching
                    $(searchInputControl).keyup(function (keyEvent) {

                    });
                }
            }
        }

        // jquery function to ensure HTML is state remains the same each time it is executed
        $.fn.outerHTML = function (s) {
            return (s)
                ? this.before(s).remove()
                : $("<p>").append(this.eq(0).clone()).html();
        }

        exports = {
            defaults: {
                id: 0,
                controlId: null,
                restUrl: null,
                searchRestUrl:null,
                restParams: null,
                allowCategorySelection: false,
                categoryPrefix: '',
                allowMultiSelect: false,
                defaultText: '',
                selectedIds: null,
                expandedIds: null,
                expandedCategoryIds: null,
                showSelectChildren: false,
                enhanceForLongLists: false,
                displayChildItemCountLabel: false,
                customDataItems: []
            },
            controls: {},
            initialize: function (options) {
                var settings,
                    accountPicker;

                if (!options.controlId) {
                    throw 'controlId must be set';
                }

                if (!options.restUrl) {
                    throw 'restUrl must be set';
                }

                if (options.enhanceForLongLists === true && !options.searchRestUrl) {
                    throw 'searchRestUrl must be set';
                }

                settings = $.extend({}, exports.defaults, options);

                if (!settings.defaultText) {
                    settings.defaultText = exports.defaults.defaultText;
                }

                accountPicker = new AccountPicker(settings);
                exports.controls[settings.controlId] = accountPicker;
                accountPicker.initialize();
            }
        };

        return exports;
    }());
}(jQuery));

