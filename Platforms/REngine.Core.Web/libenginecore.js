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
 * Get Element Size
 * @param {HTMLElement} element
 * @return {[number, number]}
 */
function get_element_size(element) {
    const {width, height} = element.getBoundingClientRect();
    return [width, height];
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
        get_element_size,
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
        cast,
        _typeof,
    };
}