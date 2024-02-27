import DriverModule from './REngine-DiligentNativeDriver.js';

let module = null;

/**
 * Allocate Memory Address on Driver Heap
 * @param {number} sizeof
 * @returns {number} Memory Address
 */
function malloc(sizeof) {
    if(sizeof === 0)
        return 0;
    return module._malloc(sizeof);
}

/**
 * Free allocated memory address on driver heap
 * @param {number} ptr_addr
 */
function free(ptr_addr) {
    if(ptr_addr)
        module._free(ptr_addr);
}

/**
 * Copy memory form .NET to Driver, Driver to .NET or Driver to Driver
 * @param {number | Object} src
 * @param {number | Object} dst
 * @param {number} sizeof
 */
function memcpy(src, dst, sizeof) {
    if(sizeof === 0)
        return;
    if(typeof src == 'object' && typeof dst == 'object')
        throw new Error('invalid memcpy call. it seem you\'re copying two span\'s. Do this operation under engine code.');
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
 * Set a default value on block of memory
 * @param {number} memory_addr
 * @param {number} value
 * @param {number} sizeof
 */
function memset(memory_addr, value, sizeof) {
    if(memory_addr === 0 || sizeof === 0)
        return;
    memory_addr = memory_addr >> 0;
    const targetMemIdx = memory_addr + sizeof;
    while(memory_addr < targetMemIdx) {
        module.HEAPU8[memory_addr] = value;
        ++memory_addr;
    }
}

/**
 * read int8 from driver memory address
 * @param {number} mem_addr
 * @returns {number}
 */
function read_i8(mem_addr) {
    return module.HEAP8[mem_addr >> 0];
}

/**
 * read int16 from driver memory address
 * @param {number} mem_addr
 * @returns {number}
 */
function read_i16(mem_addr) {
    return module.HEAP16[mem_addr >> 1];
}

/**
 * read int32 from driver memory address
 * @param {number} mem_addr
 * @returns {number}
 */
function read_i32(mem_addr) {
    return module.HEAP32[mem_addr >> 2];
}

/**
 * read float from driver memory address
 * @param {number} mem_addr
 * @returns {number}
 */
function read_float(mem_addr) {
    return module.HEAPF32[mem_addr >> 2];
}

/**
 * read double from driver memory address
 * @param {number} mem_addr
 * @returns {number}
 */
function read_double(mem_addr) {
    return module.HEAPF64[mem_addr >> 3];
}

/**
 * write int8 on driver memory address
 * @param {number} mem_addr
 * @param {number} value
 */
function write_i8(mem_addr, value) {
    if(mem_addr)
        module.HEAP8[mem_addr >> 0] = value;
}

/**
 * write int16 on driver memory address
 * @param {number} mem_addr
 * @param {number} value
 */
function write_i16(mem_addr, value) {
    if(mem_addr)
        module.HEAP16[mem_addr >> 1] = value;
}

/**
 * write int32 on driver memory address
 * @param {number} mem_addr
 * @param {number} value
 */
function write_i32(mem_addr, value) {
    if(mem_addr)
        module.HEAP32[mem_addr >> 2] = value;
}

/**
 * write float on driver memory address
 * @param {number} mem_addr
 * @param {number} value
 */
function write_float(mem_addr, value) {
    if(mem_addr)
        module.HEAPF32[mem_addr >> 2] = value;
}

/**
 * write double on driver memory address
 * @param {number} mem_addr
 * @param {number} value
 */
function write_double(mem_addr, value) {
    if(mem_addr)
        module.HEAPF32[mem_addr >> 3] = value;
}


let _lastMethodArgs = null;
function get_last_method_args() {
    return _lastMethodArgs;
}

const _funcTbl = {};
/**
 * Register JS function and retrieves a pointer to it.
 * @param {Function} callback
 * @param {string} signature
 * @return {Number} return function pointer
 */
function register_function(callback, signature) {
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
function unregister_function(func_ptr_idx) {
    const callback = _funcTbl[func_ptr_idx];
    delete _funcTbl[func_ptr_idx];
    if(callback?.dispose)
        callback.dispose();
}

function get_ptr_size() {
    return module.HEAP32.BYTES_PER_ELEMENT;
}

/**
 * allocate string on driver memory
 * @param {string} str
 * @return {number}
 */
function alloc_string(str) {
    if (!str)
        return 0;
    return module.allocateUTF8(str);
}

/**
 * read string from pointer
 * @param {number} ptr_addr
 * @return {string}
 */
function get_string(ptr_addr) {
    if (ptr_addr === 0)
        return '';
    return module.UTF8ToString(ptr_addr);
}
export async function init() {
    module = await DriverModule();
    await module.ready;
    const driverCallKeys = Object.keys(module).filter(x => x.startsWith('_rengine'));
    const driverCalls = {};

    driverCallKeys.forEach(key => driverCalls[key] = module[key]);
    const getModule = ()=> module;
    return {
        malloc,
        free,
        memcpy,
        memset,
        read_i8,
        read_i16,
        read_i32,
        read_float,
        read_double,
        write_i8,
        write_i16,
        write_i32,
        write_float,
        write_double,
        get_ptr_size,
        get_last_method_args,
        register_function,
        unregister_function,
        get_string,
        alloc_string,
        ...driverCalls,
    };
}
