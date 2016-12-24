// Prevalue Editors
angular.module("umbraco").controller("Our.Umbraco.InnerContent.Controllers.DocTypeTabPickerController", [

    "$scope",
    "innerContentService",

    function ($scope, innerContentService) {

        $scope.add = function () {
            $scope.model.value.push({
                // All stored content type aliases must be prefixed "mb" for easier recognition.
                // For good measure we'll also prefix the tab alias "mb" 
                icContentTypeAlias: "",
                icTabAlias: "",
                nameTemplate: ""
            });
        }

        $scope.selectedDocTypeTabs = function (cfg) {
            var dt = _.find($scope.model.docTypes, function (itm) {
                return itm.alias.toLowerCase() === cfg.icContentTypeAlias.toLowerCase();
            });
            var tabs = dt ? dt.tabs : [];
            if (!_.contains(tabs, cfg.icTabAlias)) {
                cfg.icTabAlias = tabs[0];
            }
            return tabs;
        }

        $scope.remove = function (index) {
            $scope.model.value.splice(index, 1);
        }

        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".icon-navigation"
        };

        innerContentService.getContentTypes().then(function (docTypes) {
            $scope.model.docTypes = docTypes;
        });

        if (!$scope.model.value) {
            $scope.model.value = [];
            $scope.add();
        }
    }
]);

angular.module("umbraco").controller("Our.Umbraco.InnerContent.Controllers.DocTypePickerController", [

    "$scope",
    "Our.Umbraco.InnerContent.Resources.InnerContentResources",

    function ($scope, icResources) {

        $scope.add = function () {
            $scope.model.value.push({
                // All stored content type aliases must be prefixed "mb" for easier recognition.
                // For good measure we'll also prefix the tab alias "mb" 
                icContentTypeAlias: "",
                nameTemplate: ""
            });
        }

        $scope.remove = function (index) {
            $scope.model.value.splice(index, 1);
        }

        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".icon-navigation"
        };

        icResources.getContentTypes().then(function (docTypes) {
            $scope.model.docTypes = docTypes;
        });

        if (!$scope.model.value) {
            $scope.model.value = [];
            $scope.add();
        }
    }
]);

// Property Editors
angular.module("umbraco").controller("Our.Umbraco.InnerContent.Controllers.InnerContentDialogController",
    [
        "$scope",
        "$interpolate",
        "formHelper",
        "contentResource",

        function ($scope) {
            $scope.item = $scope.model.dialogData.item;
        }

    ]);

// Directives
angular.module('umbraco.directives').directive('innerContentOverlay', [
    
    "innerContentService",

    function (innerContentService) {

        function link(scope, el, attr, ctrl) {

            scope.currentItem = null;

            scope.contentTypePickerOverlay = {
                view: "itempicker",
                filter: false,
                title: "Insert Content",
                show: false,
                submit: function (model) {
                    var scaffold = innerContentService.getScaffold(scope.config.scaffolds, model.selectedItem.alias);
                    scope.currentItem = innerContentService.createEditorModel(scaffold);
                    scope.closeContentTypePickerOverlay();
                    scope.openContentEditorOverlay();
                },
                close: function() {
                    scope.closeAllOverlays();
                }
            };

            scope.contentEditorOverlay = {
                view: Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + "/innercontent/views/innercontent.dialog.html",
                show: false,
                submit: function(model) {
                    if (scope.config.callback) {
                        // Convert model to basic model
                        scope.config.data.model = innerContentService.createDbModel(model.dialogData.item);

                        // Notify callback
                        scope.config.callback(scope.config.data);
                    }
                    scope.closeAllOverlays();
                },
                close: function() {
                    scope.closeAllOverlays();
                }
            };

            scope.openContentTypePickerOverlay = function() {

                if (scope.contentTypePickerOverlay.availableItems.length === 0) {
                    scope.closeAllOverlays();
                    return;
                }

                if (scope.contentTypePickerOverlay.availableItems.length === 1) {
                    var scaffold = innerContentService.getScaffold(scope.config.scaffolds, scope.contentTypePickerOverlay.availableItems[0].alias);
                    scope.currentItem = innerContentService.createEditorModel(scaffold);
                    scope.openContentEditorOverlay();
                    return;
                }

                scope.contentTypePickerOverlay.event = scope.config.event;
                scope.contentTypePickerOverlay.show = true;

            };

            scope.closeContentTypePickerOverlay = function() {
                scope.contentTypePickerOverlay.show = false;
            };

            scope.openContentEditorOverlay = function() {
                scope.contentEditorOverlay.title = "Edit item",
                    scope.contentEditorOverlay.dialogData = { item: scope.currentItem };
                scope.contentEditorOverlay.show = true;
            };

            scope.closeContentEditorOverlay = function() {
                scope.contentEditorOverlay.show = false;
            };

            scope.closeAllOverlays = function() {
                scope.closeContentTypePickerOverlay();
                scope.closeContentEditorOverlay();
                scope.config.show = false;
            };

            // Initialize
            if (scope.config) {

                // Map scaffolds to content type picker list
                scope.contentTypePickerOverlay.availableItems = scope.config.scaffolds.map(function(itm) {
                    return {
                        alias: itm.contentTypeAlias,
                        name: itm.contentTypeName,
                        icon: itm.icon
                    };

                });

                // Open relevant dialog
                if (!scope.config.data || !scope.config.data.model) {
                    scope.openContentTypePickerOverlay();
                } else {
                    var scaffold = innerContentService.getScaffold(scope.config.scaffolds, scope.config.data.model.icContentTypeAlias);
                    scope.currentItem = innerContentService.createEditorModel(scaffold, scope.config.data.model);
                    scope.openContentEditorOverlay();
                }

            }
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + '/innercontent/views/innercontent.overlay.html',
            scope: {
                config: "="
            },
            link: link
        };

        return directive;

    }
]);

// Services
angular.module("umbraco").factory('innerContentService', [

    "$q",
    "$interpolate",
    "contentResource",

    "Our.Umbraco.InnerContent.Resources.InnerContentResources",

    function ($q, $interpolate, contentResource, icResources) {

        var postProcessScaffolds = function (contentTypes, scaffolds) {

            // Sort scaffolds based on contentTypes order
            var contentTypeAliases = contentTypes.map(function (itm) {
                return itm.icAlias;
            });

            scaffolds = _.sortBy(scaffolds, [
                function (s) {
                    return contentTypeAliases.indexOf(s.contentTypeAlias);
                }
            ]);

            _.each(scaffolds, function (scaffold) {

                // Remove general properties tab
                scaffold.tabs.pop();

            });

            return scaffolds;
        }

        var self = {};

        self.getScaffolds = function (contentTypes, def, scaffolds, idx) {

            if (!def) {
                def = $q.defer();
                scaffolds = [];
                idx = 0;
            }

            var contentType = contentTypes[idx];

            contentResource.getScaffold(-20, contentType.icContentTypeAlias).then(function (scaffold) {

                // remove all tabs except the specified tab
                if (contentType.hasOwnProperty("icTabAlias"))
                {
                    var tab = _.find(scaffold.tabs, function(tab) {
                        return tab.id !== 0 && (tab.alias.toLowerCase() === contentType.icTabAlias.toLowerCase() || contentType.icTabAlias === "");
                    });
                    scaffold.tabs = [];
                    if (tab) {
                        scaffold.tabs.push(tab);
                    }
                }

                // Store the scaffold object
                scaffolds.push(scaffold);

                // Recurse
                if (idx < contentTypes.length - 1) {
                    self.getScaffolds(contentTypes, def, scaffolds, ++idx);
                } else {
                    def.resolve(postProcessScaffolds(contentTypes, scaffolds));
                    return def.promise;
                }

            }, function () {

                // Recurse
                if (idx < contentTypes.length - 1) {
                    self.getScaffolds(contentTypes, def, scaffolds, ++idx);
                } else {
                    def.resolve(postProcessScaffolds(contentTypes, scaffolds));
                    return def.promise;
                }
            });

            //def.notify();
            return def.promise;
        };

        self.getScaffold = function (scaffolds, alias) {
            return _.find(scaffolds, function (scaffold) {
                return scaffold.contentTypeAlias === alias;
            });
        }

        self.populateName = function (itm, idx, contentTypes) {

            var contentType = _.find(contentTypes, function(itm2) {
                return itm2.icContentTypeAlias === itm.icContentTypeAlias;
            });

            var nameTemplate = contentType.nameTemplate || "Item {{$index}}";
            var nameExp = $interpolate(nameTemplate);

            if (nameExp) {
                    
                // Inject temporary index property
                itm.$index = idx;

                // Execute the name expression
                var newName = nameExp(itm);
                if (newName && (newName = $.trim(newName)) && itm.name !== newName) {
                    itm.name = newName;
                }

                // Remove temporary index property
                delete itm.$index;
            }

        }

        self.getContentTypes = function () {
            return icResources.getContentTypes();
        }

        self.getContentTypeIcons = function (aliases) {
            return icResources.getContentTypeIcons(aliases);
        }

        self.createEditorModel = function (scaffold, existing) {

            var item = angular.copy(scaffold);

            item.key = existing && existing.key ? existing.key : self.generateUid();
            item.icContentTypeAlias = scaffold.contentTypeAlias;
            item.name = existing && existing.name ? existing.name : "Untitled";

            for (var t = 0; t < item.tabs.length; t++) {
                var tab = item.tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    if (existing && existing.hasOwnProperty(prop.alias)) {
                        prop.value = existing[prop.alias];
                    }
                }
            }

            return item;
        }

        self.createDbModel = function (model) {

            var dbModel = {
                key: model.key,
                name: model.name,
                icon: model.icon,
                icContentTypeAlias: model.contentTypeAlias
            };

            for (var t = 0; t < model.tabs.length; t++) {
                var tab = model.tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    if (typeof prop.value !== "function") {
                        dbModel[prop.alias] = prop.value;
                    }
                }
            }

            return dbModel;
        }

        self.createDefaultDbModel = function(scaffold) {
            var editorModel = self.createEditorModel(scaffold);
            return self.createDbModel(editorModel);
        }

        // Helpful methods
        var lut = []; for (var i = 0; i < 256; i++) { lut[i] = (i < 16 ? '0' : '') + (i).toString(16); }
        self.generateUid = function () {
            var d0 = Math.random() * 0xffffffff | 0;
            var d1 = Math.random() * 0xffffffff | 0;
            var d2 = Math.random() * 0xffffffff | 0;
            var d3 = Math.random() * 0xffffffff | 0;
            return lut[d0 & 0xff] + lut[d0 >> 8 & 0xff] + lut[d0 >> 16 & 0xff] + lut[d0 >> 24 & 0xff] + '-' +
              lut[d1 & 0xff] + lut[d1 >> 8 & 0xff] + '-' + lut[d1 >> 16 & 0x0f | 0x40] + lut[d1 >> 24 & 0xff] + '-' +
              lut[d2 & 0x3f | 0x80] + lut[d2 >> 8 & 0xff] + '-' + lut[d2 >> 16 & 0xff] + lut[d2 >> 24 & 0xff] +
              lut[d3 & 0xff] + lut[d3 >> 8 & 0xff] + lut[d3 >> 16 & 0xff] + lut[d3 >> 24 & 0xff];
        }

        return self;
    }

]);