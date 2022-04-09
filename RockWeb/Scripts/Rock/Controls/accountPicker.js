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
            this.checkedNodes = [];
            this.searchMode = false;
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

                $control.find('.picker-preview').hide();
                $control.find('.picker-treeview').hide();

                if (treeOptions.allowMultiSelect) {
                    $control.find('.picker-preview').remove();
                    $control.find('.picker-treeview').remove();
                }

                this.updateScrollbar();
            },
            initializeEventHandlers: function () {
                var self = this,
                    $control = $('#' + this.options.controlId),
                    $spanNames = $control.find('.selected-names'),
                    $hfItemIds = $control.find('.js-item-id-value'),
                    $hfExpandedIds = $control.find('.js-initial-item-parent-ids-value'),
                    $hfItemNames = $control.find('.js-item-name-value');

                // Bind tree events
                $control.find('.treeview')
                    .on('rockTree:selected', function (e) {
                        self.showLinks();
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
                        var rockTree = $control.find('.treeview').data('rockTree');
                        self.createSearchControl();
                        self.showActiveMenu();
                        self.scrollToSelectedItem();

                        if ($hfItemIds && $hfItemIds.val().length > 0) {
                            rockTree.setSelected($hfItemIds.val().split(','));
                        }

                        self.showLinks();
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

                    $control.find('.picker-preview').hide();
                    $control.find('.picker-treeview').show();

                    var rockTree = $control.find('.treeview').data('rockTree');

                    // Get all of the current rendered items
                    var $viewport = $control.find('.viewport');

                    if (rockTree.selectedNodes && rockTree.selectedNodes.length > 0) {
                        var listHtml = '';
                        rockTree.selectedNodes.forEach(function (node) {
                            console.debug(node);
                            listHtml +=
                                '<div id="preview-item-' + node.id + '" class="container-fluid">' +
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
                                '         <a id="lnk-remove-preview-' + node.id + '" title="Remove From Preview" class="btn btn-link text-muted js-remove-preview" data-id="' + node.id + '"> <i class="fa fa-times"></i></a>' +
                                '       </div>' +
                                ' </div>' +
                                '</div>';
                        });

                        // Display preview list
                        var listHtmlView = '<ul class="rocktree">' +
                            listHtml +
                            '</ul>';

                        $viewport.html(listHtmlView);
                        // Wire up remove event and remove from dataset

                        $control.find('.js-remove-preview').on('click', function (e) {
                            var nodeId = $(this).attr('data-id');
                            $('#preview-item-' + node.nodeId).remove();

                            rockTree.selectedNodes = rockTree.selectedNodes.filter(function (fNode) {
                                return fNode.id !== nodeId;
                            });

                            //ToDo: remove items from selection

                        });

                        //previewData.forEach(function (node) {

                        //    $('#lnk-remove-preview-' + node.nodeId).on('click', function (e) {

                        //        $('#preview-item-' + node.nodeId).remove();

                        //        //previewData = previewData.filter(function (fNode) {
                        //        //    return fNode.nodeId !== node.nodeId;
                        //        //});

                        //        //// Re-select the items on the tree if it have removed them all
                        //        //if (!previewData || previewData.length === 0) {
                        //        //    $control.find('.item-picker-search').show();
                        //        //}
                        //    });
                        //});
                    }
                });

                // Tree View link click
                $control.find('.picker-treeview').on('click', function () {
                    var rockTree = $control.find('.treeview').data('rockTree');

                    var $showPickerActive = $control.find('.js-picker-showactive-value');
                    var $searchValueField = $control.find('.js-existing-search-value');

                    if (self.checkedNodes && self.checkedNodes.length > 0) {
                        var restUrl = self.options.getParentIdsUrl
                        var restUrlParams = self.checkedNodes.map(function (v) { return 'ids=' + v.nodeId }).join('&');

                        restUrl = restUrl + '?' + restUrlParams;

                        // Get the ancestor ids so the tree will expand
                        $.getJSON(restUrl, function (data, status) {

                            console.debug('data', data);

                            if (data && status === 'success') {
                                var selectedIds = [];
                                var expandedIds = [];

                                $.each(data, function (key, value) {
                                    selectedIds.push(key);

                                    value.forEach(function (kval) {
                                        if (!expandedIds.find(function (expVal) {
                                            return expVal === kval
                                        })) {
                                            expandedIds.push(kval);
                                        }
                                    });
                                });

                                console.debug('selectedIds', selectedIds);
                                console.debug('expandedIds', expandedIds);

                                var firePostBack = false;

                                if (expandedIds && expandedIds.length > 0) {
                                    $hfExpandedIds.val(expandedIds.join(','));
                                    rockTree.expandedIds = $hfExpandedIds.val();
                                    firePostBack = true;
                                }

                                if (selectedIds && selectedIds.length > 0) {
                                    $hfItemIds.val(selectedIds.join(','));
                                    rockTree.expandedIds = $hfExpandedIds.val();
                                    firePostBack = true;
                                }

                                if (firePostBack) {
                                    $searchValueField.val('');
                                    $showPickerActive.val('true');
                                    doPostBack();
                                }
                            }
                        });
                    }

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
            showActiveMenu: function () {
                var $control = $('#' + this.options.controlId);
                var showPickerActive = $control.find('.js-picker-showactive-value').val();
                var isActive = showPickerActive && showPickerActive === 'true' ? true : false;

                if (isActive) {
                    $('.picker-label').click();
                }

                $control.find('.js-picker-showactive-value').val('')
            },
            addChecked: function (nodes) {
                var self = this;
                if (!self.checkedNodes) {
                    self.checkedNodes = [];
                }

                if ($.isArray(nodes)) {
                    self.checkedNodes = nodes;
                }
                else {
                    self.checkedNodes.push(nodes);
                }
            },
            removeChecked: function (nodeId) {
                if (!this.checkedNodes || !nodeId) {
                    this.selectedNodes = [];
                }
                this.selectedNodes = this.checkedNodes.filter(function (v, idx) {
                    return v.nodeId !== nodeId;
                });
            },
            showLinks: function () {
                var $control = $('#' + this.options.controlId);
                var rockTree = $control.find('.treeview').data('rockTree');

                var hasSelectedNode = rockTree.selectedNodes && rockTree.selectedNodes.length > 0;

                console.debug(hasSelectedNode);

                if (self.searchMode && hasSelectedNode) {
                    $control.find('.picker-treeview').show();
                    $control.find('.picker-preview').hide();
                }
                else if (!self.searchMode && hasSelectedNode) {
                    $control.find('.picker-preview').show();
                    $control.find('.picker-treeview').hide();
                }
                else {
                    $control.find('.picker-preview').hide();
                    $control.find('.picker-treeview').hide();
                }
            },
            findNodes: function (allNodes, selectNodeIds) {
                if (selectNodeIds) {
                    if ($.isArray(selectNodeIds)) {
                        const filterArray = (nodes, ids) => {
                            const filteredNodes = nodes.filter(node => {
                                return ids.indexOf(node.nodeId) >= 0;
                            });
                            return filteredNodes;
                        };

                        return filterArray(allNodes, selectNodeIds);
                    }
                    else {
                        return allNodes.filter(node => {
                            return selectNodeIds.indexOf(node.nodeId) >= 0;
                        });
                    }
                }
            },
            createSearchControl: function () {
                var self = this;
                var controlId = self.options.controlId;

                var $control = $('#' + controlId);

                if (self.options.enhanceForLongLists === true) {

                    // A hidden value to store the current search criteria to be read on post-backs
                    var $searchValueField = $control.find('.js-existing-search-value');

                    var $searchControl =
                        $('<div class="rocktree-drawer form-group js-group-search" style="display: none;">' +
                            '	<span class="control-label d-none">Search</span>' +
                            '	<div id="pnlSearch_' + controlId + '" class="input-group js-search-panel" > ' +
                            '		<input id="tbSearch_' + controlId + '" type="text" placeholder="Quick Find" class="form-control input-sm" />' +
                            '		<span class="input-group-btn">' +
                            '			<a id="btnSearch_' + controlId + '" class="btn btn-default btn-sm"><i class="fa fa-search"></i></a>' +
                            '		</span>' +
                            '	</div>' +
                            '</div><div class="mb-5"></div>');

                    // Get all of the rendered items
                    var treeView = $control.find('.treeview').html();

                    var $treeView = $control.find('.treeview');
                    var $overview = $control.find('.overview');
                    var $viewport = $control.find('.viewport');

                    // Added this check to prevent rendering call from duping the element
                    if ($viewport.find('.js-search-panel').length === 0) {
                        // Add the search control after rendering
                        $overview.prepend($searchControl.html());
                    }

                    var $searchInputControl = $('#tbSearch_' + controlId);

                    $('#btnSearch_' + controlId).off('click').on('click', function () {

                        var searchKeyword = $searchInputControl.val();

                        if (searchKeyword && searchKeyword.length > 0) {
                            var searchRestUrl = self.options.searchRestUrl;
                            var restUrlParams = self.options.restParams + '/' + searchKeyword;

                            searchRestUrl += restUrlParams;

                            $.getJSON(searchRestUrl, function (data, status) {

                                if (data && status === 'success') {
                                    $treeView.html('');
                                }
                                else {
                                    $overview.html(treeView);
                                    return;
                                }

                                // Create the search results node object
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

                                        var disabledCheck = '';
                                        var mutedText = '';
                                        if (!v.isActive || v.isActive === false) {
                                            disabledCheck = ' disabled';
                                            mutedText = ' text-muted';
                                        }

                                        var inputHtml = '<input type="radio" data-id="' + v.nodeId + '" class="checkbox js-opt-search"' + disabledCheck + '>';
                                        if (self.options.allowMultiSelect) {
                                            inputHtml = '<input type="checkbox" data-id="' + v.nodeId + '" class="checkbox js-chk-search"' + disabledCheck + '>';
                                        }

                                        listHtml +=
                                            '<div id="divSearchItem" class="container-fluid">' +
                                            '      <div class="row">' +
                                            '        <div class="col-xs-1 pr-0 pt-2">' +
                                            inputHtml +
                                            '        </div>' +
                                            '        <div class="col-xs-11 pl-0">' +
                                            '              <h5><span class="rocktree-node-name-text text-color' + mutedText + '">' + v.title + '</span></br>' +
                                            '              <span class="text-muted"><small>' + v.path.replaceAll('^', '<i class="fa fa-chevron-right pl-1 pr-1" aria-hidden="true"></i>') + '</small></span></h5>' +
                                            '        </div>' +
                                            '     </div>' +
                                            '</div>';
                                    });

                                    // add the results to the panel
                                    $treeView.html(listHtml);

                                    $viewport.addClass('overflow-auto');

                                    // Handle multi item check selection
                                    $control.find('.js-chk-search').off('change').on('change', function () {
                                        var $allChecked = $control.find('.js-chk-search:checked');
                                        var checkedVals = $allChecked.map(function () {
                                            return $(this).attr('data-id');
                                        }).get();

                                        // ToDo: Get all the node id ancestors for multi select via api

                                        if (checkedVals && checkedVals.length > 0) {
                                            $control.find('.picker-treeview').show();
                                            self.addChecked(self.findNodes(nodes, checkedVals));
                                        }
                                        else {
                                            $control.find('.picker-treeview').hide();
                                            self.removeChecked();
                                        }
                                    });

                                    // Handle single item radio selection
                                    $control.find('.js-opt-search').off('change').on('change', function () {
                                        var thisNodeId = $(this).attr('data-id');

                                        //prevent multi select
                                        $control.find('.js-opt-search:not([data-id=' + thisNodeId + '])').prop('checked', false);

                                        var $allChecked = $control.find('.js-opt-search:checked');
                                        var checkedVals = $allChecked.map(function () {
                                            return $(this).attr('data-id');
                                        }).get();

                                        if (checkedVals && checkedVals.length > 0) {
                                            self.addChecked(self.findNodes(nodes, checkedVals));
                                        }
                                        else {
                                            self.removeChecked();
                                        }

                                        //ToDo: Get all the node id ancestors for single select via api

                                        console.debug(self.checkedNodes);
                                    });
                                }
                            });
                        }

                    });


                    // If we have an existing search value on postback
                    if ($searchValueField.length > 0 && $searchValueField.val().length > 0) {
                        $searchInputControl.val($searchValueField.val());
                        $('#btnSearch_' + controlId).click();
                    }

                    // Handle the input searching
                    $searchInputControl.keyup(function (keyEvent) {
                        keyEvent.preventDefault();

                        var searchKeyword = $searchInputControl.val();

                        if (!searchKeyword || searchKeyword.length === 0) {
                            $treeView.html(treeView);
                            $viewport.removeClass('overflow-auto');
                        }

                    }).keydown(function (keyEvent) {
                        var searchKeyword = $searchInputControl.val();
                        $searchValueField.val(searchKeyword);

                        if ($searchInputControl.val().length > 0) {
                            self.searchMode = true;
                        }
                        else {
                            self.searchMode = false;
                        }

                        if (keyEvent.which === 13) {
                            keyEvent.preventDefault();
                            $('#btnSearch_' + controlId).click();
                        }
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
                searchRestUrl: null,
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

