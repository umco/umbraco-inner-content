angular.module("umbraco.resources").factory("Our.Umbraco.InnerContent.Resources.InnerContentResources",
    function ($q, $http, umbRequestHelper) {
        return {
            getAllContentTypes: function () {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/GetAllContentTypes",
                        method: "GET"
                    }),
                    "Failed to retrieve content types"
                );
            },
            getContentTypesByGuid: function (guids) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/GetContentTypesByGuid",
                        method: "GET",
                        params: { guids: guids }
                    }),
                    "Failed to retrieve content types"
                );
            },
            getContentTypesByAlias: function (aliases) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/GetContentTypesByAlias",
                        method: "GET",
                        params: { aliases: aliases }
                    }),
                    "Failed to retrieve content types"
                );
            },
            getContentTypeIconsByGuid: function (guids) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/GetContentTypeIconsByGuid",
                        method: "GET",
                        params: { guids: guids }
                    }),
                    "Failed to retrieve content type icons"
                );
            },
            getContentTypeScaffoldByGuid: function (guid) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/GetContentTypeScaffoldByGuid",
                        method: "GET",
                        params: { guid: guid }
                    }),
                    "Failed to retrieve content type scaffold by Guid"
                );
            },
            getContentTypeScaffoldByBlueprintId: function (blueprintId) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/GetContentTypeScaffoldByBlueprintId",
                        method: "GET",
                        params: { blueprintId: blueprintId }
                    }),
                    "Failed to retrieve content type scaffold by blueprint Id"
                );
            }
        };
    });