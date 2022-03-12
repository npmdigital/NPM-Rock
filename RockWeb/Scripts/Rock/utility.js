(function () {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.utility = (function () {
        var _utility = {},
            exports = {

                setContext: function (restController, entityId) {
                    // Get the current block instance object
                    $.ajax({
                        type: 'PUT',
                        url: Rock.settings.get('baseUrl') + 'api/' + restController + '/SetContext/' + entityId,
                        success: function (getData, status, xhr) {
                        },
                        error: function (xhr, status, error) {
                            alert(status + ' [' + error + ']: ' + xhr.responseText);
                        }
                    });
                },

                uuidv4: function () {
                    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
                        return v.toString(16);
                    });
                },
                encodeHTML: function (str) {
                    return str.replace(/([\u00A0-\u9999<>&])(.|$)/g, function (full, char, next) {
                        if (char !== '&' || next !== '#') {
                            if (/[\u00A0-\u9999<>&]/.test(next))
                                next = '&#' + next.charCodeAt(0) + ';';

                            return '&#' + char.charCodeAt(0) + ';' + next;
                        }

                        return full;
                    });
                },
                decodeHTML: function (str) {
                    return str.replace(/&#([0-9]+);/g, function (full, int) {
                        return String.fromCharCode(parseInt(int));
                    });
                },
                isArray: function (arr) {
                    return arr.constructor.toString().indexOf("Array") > -1;
                },
                getObjects: function (obj, key, val, debug) {
                    var isArrayResult = this.isArray(obj);
                    if (debug) {
                        console.debug('Rock.utility.getObjects=>k,v', { key, val });
                        console.debug('Rock.utility.getObjects=>isArray', isArrayResult);
                    }
                    var returnVal;
                    if (isArrayResult) {
                        returnVal = obj.filter(function (o) {
                            var objPropValue = o['' + key + ''];
                            if (debug) {
                                console.debug('Rock.utility.getObjects=>objPropValue', objPropValue);
                            }
                            if (objPropValue && objPropValue === val) {
                                return true;
                            }
                        });
                        if (debug) {
                            console.debug('Rock.utility.getObjects=>returnVal', returnVal);
                        }
                    }
                    return returnVal;
                },
                getObject: function (obj, key, val, debug) {

                    var isArrayResult = this.isArray(obj);

                    if (debug) {
                        console.debug('Rock.utility.getObject=>k,v', { key, val });
                        console.debug('Rock.utility.getObject=>isArray', isArrayResult);
                    }

                    var returnVal;

                    if (isArrayResult) {
                        returnVal = obj.find(function (o, i) {

                            var objPropValue = o['' + key + ''];

                            if (debug) {
                                console.debug('Rock.utility.getObject=>objPropValue', objPropValue);
                            }

                            if (objPropValue && objPropValue === val) {
                                return true;
                            }
                        });

                        if (debug) {
                            console.debug('Rock.utility.getObject=>returnVal', returnVal);
                        }
                    }

                    return returnVal;
                },
                objectHasValue: function (obj, key, val, debug) {
                    var returnVal = false;

                    var checkObj = this.getObject(obj, key, val, debug);

                    if (checkObj) {
                        returnVal = true;
                    }
                    if (debug) {
                        console.debug('Rock.utility.objectHasValue=>returnVal', returnVal);
                    }
                    return returnVal;
                },
                listToTree: function (list) {
                    var map = {}, node, roots = [], i;

                    for (i = 0; i < list.length; i += 1) {
                        map[list[i].id] = i; // initialize the map
                        list[i].children = []; // initialize the children
                    }

                    for (i = 0; i < list.length; i += 1) {
                        node = list[i];
                        if (node.parentId !== '0') {
                            // if you have dangling branches check that map[node.parentId] exists
                            list[map[node.parentId]].children.push(node);
                        } else {
                            roots.push(node);
                        }
                    }
                    return roots;
                },
                toCamelCase: function (str) {
                    return str.replace(/(?:^\w|[A-Z]|\b\w)/g, function (word, index) {
                        return index === 0 ? word.toLowerCase() : word.toUpperCase();
                    }).replace(/\s+/g, '');
                },
                stripNonNumber: function (str) {
                    var retVal = str.replace(/\D/g, '');

                    return retVal;
                },
                encodeUnicode: function (str) {
                    // first we use encodeURIComponent to get percent-encoded UTF-8,
                    // then we convert the percent encodings into raw bytes which
                    // can be fed into btoa.
                    return btoa(encodeURIComponent(str).replace(/%([0-9A-F]{2})/g,
                        function toSolidBytes(match, p1) {
                            return String.fromCharCode('0x' + p1);
                        }));
                },
                decodeUnicode: function (str) {
                    // Going backwards: from bytestream, to percent-encoding, to original string.
                    return decodeURIComponent(atob(str).split('').map(function (c) {
                        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
                    }).join(''));
                },
                isDomElement: function (input) {
                    if (typeof input === 'string') {
                        return false;
                    } else {
                        if ($(input).length) {
                            return true;
                        }
                    }

                    return false;
                }
            };

        return exports;
    }());
}());
