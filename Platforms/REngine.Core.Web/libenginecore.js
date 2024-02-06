/**
 * Creates an event loop method
 * Execute result method to cancel event loop
 * @param callback
 * @returns {(function(): void)|*}
 */
function make_frame_loop(callback) {
    let stop = false;
    const runFrame = () => {
        if (stop) {
            callback?.dispose();
            return;
        }

        callback();
        window.requestAnimationFrame(runFrame);
    };

    window.requestAnimationFrame(runFrame);
    return () => {
        stop = false;
    };
}

/**
 * Query Selector Element
 * @param {string} query
 * @returns {HTMLElement}
 */
function query_selector(query) {
    return document.querySelector(query);
}

/**
 * Get Parent Element
 * @param element
 * @return {HTMLElement?}
 */
function get_element_parent(element) {
    return element.parentElement;
}
/**
 * Get Element Size
 * @param {HTMLElement} element
 * @return {[number, number]}
 */
function get_element_size(element) {
    const {width, height} = element.getBoundingClientRect();
    return [width, height];
}

/**
 * Set Element Size
 * @param {HTMLElement | HTMLCanvasElement} element
 * @param {number} width
 * @param {number} height
 */
function set_element_size(element, width, height) {
    if(element instanceof HTMLCanvasElement) {
        element.width = width;
        element.height = height;
    } else {
        element.style.width = width+'px';
        element.style.height = height+'px';
    }
}
/**
 * Listen to element size changes
 * @param {HTMLElement} element
 * @param {Function} callback
 */
function on_resize_event(element, callback) {
    if (!element || !callback)
        return;

    const resizeObserver = new ResizeObserver(() => {
        callback();
    });
    resizeObserver.observe(element, {box: 'content-box'});
    return ()=> {   
        resizeObserver.disconnect();
        callback?.dispose();
    };
}

const _arrayList = [];
const _freeArrayList = [];
/**
 * .NET Exclusive
 * Allocates an JavaScript array
 * @returns {number} array id
 */
function array_new() {
    // Array is serialized every time 
    // on .NET, this means if we wan't the truly
    // JS array working on .NET, we must handle 
    // array operations entirely on JS.
    if(_freeArrayList.length === 0) {
        const result = _arrayList.length;
        _arrayList.push([]);
        return result;
    }
    return _freeArrayList.pop();
}

/**
 * Frees allocated JavaScript array
 * @param {number} arr_id
 */
function array_free(arr_id) {
    _freeArrayList.push(arr_id);
    _arrayList[arr_id] = []; // Clear Array
}

/**
 * Get native array
 * @param arr_id
 */
function array_get_native(arr_id) {
    return _arrayList[arr_id];
}
/**
 * Push object on JavaScript array
 * @param {number} arr_id
 * @param {any} item
 */
function array_push(arr_id, item) { 
    _arrayList[arr_id].push(item);
}

/**
 * Get item at given index from array
 * @param {number} arr_id
 * @param {number} index
 * @returns {any}
 */
function array_get(arr_id, index) {
    return _arrayList[arr_id][index];
}

/**
 * Set item at given index on array
 * @param {number} arr_id
 * @param {number} index
 * @param {any} value
 */
function array_set(arr_id, index, value) {
    _arrayList[arr_id][index] = value;
}

/**
 * Clear an array
 * @param {number} arr_id
 */
function array_clear(arr_id) {
    _arrayList[arr_id] = [];
}

/**
 * Get length from an array
 * @param {number} arr_id
 */
function array_length(arr_id) {
    return _arrayList[arr_id].length;
}

/**
 * Get IndexOf item on array
 * @param {number} arr_id
 * @param {any} item
 */
function array_indexof(arr_id, item) {
    return _arrayList[arr_id].indexOf(item);
}

/**
 * Insert item at given index
 * @param {number} arr_id
 * @param {number} idx
 * @param {any} item
 */
function array_insert(arr_id, idx, item) {
    _arrayList[arr_id].splice(idx, 0, item);
}

/**
 * Remove item at given index on array
 * @param {number} arr_id
 * @param {number} idx
 */
function array_remove(arr_id, idx) {
    _arrayList[arr_id].splice(idx, 1);
}
/**
 * Log value on console
 * @param {number} arr_id
 */
function console_log(arr_id) {
    console.log.apply(console, _arrayList[arr_id]);
}

/**
 * Log warning value on console
 * @param {number} arr_id
 */
function console_warn(arr_id) {
    console.warn.apply(console, _arrayList[arr_id]);
}

/**
 * Log error value on console
 * @param {number} arr_id
 */
function console_err(arr_id) {
    console.error.apply(console, _arrayList[arr_id]);
}

/**
 * get the length of sessionStorage
 * @returns {number}
 */
function session_storage_length() {
    return sessionStorage.length;
}

/**
 * gets the keys in sessionStorage
 * @returns {string[]}
 */
function session_storage_keys(){
    const arr = new Array(sessionStorage.length);
    let i = arr.length;
    while(i--)
        arr[i] = sessionStorage.key(i);
    return arr;
}

/**
 * sets a value in sessionStorage
 * @param {string} key
 * @param {string} value
 */
function session_storage_set(key, value) {
    sessionStorage.setItem(key, value);
}

/**
 * gets a value from sessionStorage
 * @param key
 * @returns {string}
 */
function session_storage_get(key) {
    return sessionStorage.getItem(key) ?? '';
}

/**
 * removes an item from sessionStorage
 * @param {string} key
 */
function session_storage_remove(key) {
    sessionStorage.removeItem(key);
}

/**
 * clear all items from sessionStorage
 */
function session_storage_clear() {
    sessionStorage.clear();
}

/**
 * Test if key exists in sessionStorage
 * @param {string} key
 * @returns {boolean}
 */
function session_storage_contains(key) {
    return Boolean(sessionStorage.getItem(key));
}

/**
 * get the length of localStorage
 * @returns {number}
 */
function local_storage_length() {
    return localStorage.length;
}

/**
 * gets the keys in localStorage
 * @returns {string[]}
 */
function local_storage_keys(){
    const arr = new Array(localStorage.length);
    let i = arr.length;
    while(i--)
        arr[i] = localStorage.key(i);
    return arr;
}

/**
 * sets a value in localStorage
 * @param {string} key
 * @param {string} value
 */
function local_storage_set(key, value) {
    localStorage.setItem(key, value);
}

/**
 * Gets a value from localStorage
 * @param {string} key
 * @returns {string}
 */
function local_storage_get(key) {
    return localStorage.getItem(key) ?? '';
}

/**
 * Removes an item from localStorage
 * @param {string} key
 */
function local_storage_remove(key) {
    localStorage.removeItem(key);
}

/**
 * clears all items from localStorage
 */
function local_storage_clear() {
    localStorage.clear();
}

/**
 * tests if key exists in localStorage
 * @param {string} key
 * @returns {boolean}
 */
function local_storage_contains(key) {
    return Boolean(localStorage.getItem(key));
}

/**
 * Util call used on .NET to convert
 * JavaScript objects to managed objects
 * @param obj
 */
function cast(obj) {
    return obj;
}

/**
 * TypeOf object
 * @param obj
 * @returns {"undefined"|"object"|"boolean"|"number"|"string"|"function"|"symbol"|"bigint"}
 */
function _typeof(obj) {
    return typeof obj;
}
export function init() {
    return {
        make_frame_loop,
        query_selector,
        get_element_parent,
        get_element_size,
        set_element_size,
        on_resize_event,
        array_new,
        array_free,
        array_get_native,
        array_push,
        array_get,
        array_set,
        array_length,
        array_clear,
        array_insert,
        array_indexof,
        array_remove,
        console_log,
        console_warn,
        console_err,
        session_storage_length,
        session_storage_keys,
        session_storage_set,
        session_storage_get,
        session_storage_remove,
        session_storage_clear,
        session_storage_contains,
        local_storage_length,
        local_storage_keys,
        local_storage_set,
        local_storage_get,
        local_storage_remove,
        local_storage_clear,
        local_storage_contains,
        cast,
        _typeof,
    };
}