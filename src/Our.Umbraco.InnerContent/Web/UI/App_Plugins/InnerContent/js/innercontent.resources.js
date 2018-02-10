angular.module('umbraco.resources').factory('Our.Umbraco.InnerContent.Resources.InnerContentResources',
    function ($q, $http, umbRequestHelper) {
        return {
            getContentTypes: function () {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/GetContentTypes",
                        method: "GET"
                    }),
                    'Failed to retrieve content types'
                );
            },
            getContentTypeInfos: function (guids) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/GetContentTypeInfos",
                        method: "GET",
                        params: { guids: guids }
                    }),
                    'Failed to retrieve content types'
                );
            },
            getContentTypeIcons: function (guids) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/GetContentTypeIcons",
                        method: "GET",
                        params: { guids: guids }
                    }),
                    'Failed to retrieve content type icons'
                );
            },
            getContentTypeScaffold: function (guid) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/InnerContent/InnerContentApi/getContentTypeScaffold",
                        method: "GET",
                        params: { guid: guid }
                    }),
                    'Failed to retrieve content type scaffold'
                );
            }
        };
    });