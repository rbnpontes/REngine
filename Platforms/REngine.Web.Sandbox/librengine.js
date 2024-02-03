import DriverModule from './REngine-DiligentNativeDriver.js';

let module = null;

/**
 * Allocate driver memory
 * @param {Number} size
 */
function malloc(size) {
    return module._malloc(size);
}

/**
 * Free allocated driver memory
 * @param {Number} ptr_addr
 */
function free(ptr_addr) {
    if(ptr_addr)
        module._free(ptr_addr);
}

/**
 * Read Int32 from Pointer Address
 * @param {Number} ptr_addr
 */
function readI32(ptr_addr) {
    return module.HEAP32[ptr_addr >> 2];
}

/**
 * Read Float from Pointer Address
 * @param ptr_addr
 */
function readF32(ptr_addr) {
    return module.HEAPF32[ptr_addr >> 2];
}

/**
 * Write Int32 on Pointer Address
 * @param {Number} ptr_addr
 * @param {Number} value
 */
function writeI32(ptr_addr, value) {
    module.HEAP32[ptr_addr >> 2] = value;
}

/**
 * Write Float on Pointer Address
 * @param {Number} ptr_addr
 * @param {Number} value
 */
function writeF32(ptr_addr, value) {
    module.HEAPF32[ptr_addr] = value;
}

/**
 * Copy memory from .NET to Driver and Vice verse
 * @param {number | Object} src
 * @param {number | Object} dst
 * @param {number} sizeof
 */
function memcpy(src, dst, sizeof) {
    if (sizeof === 0)
        return;
    if (typeof src == 'object' && typeof dst == 'object')
        throw new Error('invalid memcpy call. it seems you\'re copying two span\'s. Do this operation under .NET');
    if (typeof src == 'number' && typeof dst == 'object') {
        const ptr_idx = src >> 0;
        const buffer = module.HEAPU8.subarray(ptr_idx, ptr_idx + sizeof);
        dst.set(buffer, 0);
    } else if (typeof src == 'object' && typeof dst == 'number') {
        const ptr_idx = dst >> 0;
        const buffer = module.HEAPU8.subarray(ptr_idx, ptr_idx + sizeof);
        src.copyTo(buffer, 0);
    } else if (typeof src == 'number' && typeof dst == 'number') {
        const src_idx = src >> 0;
        const dst_idx = dst >> 0;
        const srcBuffer = module.HEAPU8.sub(src_idx, src_idx + sizeof);
        module.HEAPU8.set(srcBuffer, dst_idx);
    }
}

/**
 * set a default value on block of memory
 * @param {number} ptr
 * @param {number} value
 * @param {number} sizeof
 */
function memset(ptr, value, sizeof) {
    if(ptr === 0 || sizeof === 0)
        return;
    
    ptr = ptr >> 0;
    const targetPtrIdx = ptr + sizeof;
    while(ptr < targetPtrIdx) {
        module.HEAPU8[ptr] = value;
        ++ptr; 
    }
}


let _lastMethodArgs = null;
function getLastMethodArgs() {
    return _lastMethodArgs;
}

const _funcTbl = {};
/**
 * Register JS function and retrieves a pointer to it.
 * @param {Function} callback
 * @param {string} signature
 * @return {Number} return function pointer
 */
function registerFunction(callback, signature) {
    const funcPtr = module.addFunction((...args)=> {
        _lastMethodArgs = args;
        callback();
    }, signature);
    _funcTbl[funcPtr] = callback;
    return funcPtr;
}

/**
 * Unregister JS function
 * @param {Number} func_ptr_idx
 */
function unregisterFunction(func_ptr_idx) {
    const callback = _funcTbl[func_ptr_idx];
    delete _funcTbl[func_ptr_idx];
    if(callback?.dispose)
        callback.dispose();
}

function getPtrSize() {
    return module.HEAP32.BYTES_PER_ELEMENT;
}

/**
 * allocate string on driver memory
 * @param {string} str
 */
function allocString(str) {
    if (!str)
        return;
    return module.allocateUTF8(str);
}

/**
 * read string from pointer
 * @param {number} ptr_addr
 * @return {string}
 */
function getString(ptr_addr) {
    if (ptr_addr === 0)
        return '';
    return module.UTF8ToString(ptr_addr);
}

/**
 * Query Selector Element
 * @param {string} query
 * @return {HTMLElement}
 */
function querySelector(query) {
    return document.querySelector(query);
}

/**
 * Get Element Size
 * @param {HTMLElement} element
 */
function getElementSize(element) {
    const { width, height } = element.getBoundingClientRect();
    return [width, height];
}

/**
 * Listen to element size changes
 * @param {HTMLElement} element
 * @param {Function} callback
 */
function listenResizeEvent(element, callback) {
    if(!element || !callback)
        return;
    const resizeObserver = new ResizeObserver(()=> {
        callback();
    });
    resizeObserver.observe(element, {box: 'content-box'});
    return ()=> {
        resizeObserver.disconnect();
        if(callback?.dispose)
            callback.dispose();
    };
}

function makeEventLoop(callback) {
    let stop = false;
    const runFrame = ()=> {
        if(stop) {
            callback.dispose();
            return;
        }
        
        callback();
        window.requestAnimationFrame(runFrame);
    };
    
    window.requestAnimationFrame(runFrame);
    return ()=> {
        stop = true;  
    };
}
export async function init() {
    module = await DriverModule();
    await module.ready;
    const driverCallKeys = Object.keys(module).filter(x => x.startsWith('_rengine'));
    const driverCalls = {};

    driverCallKeys.forEach(key => driverCalls[key] = module[key]);

    return {
        malloc,
        free,
        readI32,
        readF32,
        writeI32,
        writeF32,
        getPtrSize,
        memcpy,
        memset,
        getLastMethodArgs,
        registerFunction,
        unregisterFunction,
        getString,
        allocString,
        querySelector,
        getElementSize,
        listenResizeEvent,
        makeEventLoop,
        ...driverCalls
    };
}
