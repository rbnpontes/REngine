#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <filesystem>

#include <slang/slang.h>

#include <rengine/rengine.h>
#include <ryml.hpp>
#include <ryml_std.hpp>

using namespace slang;

class material_compiler_exception: public std::exception {
public:
    material_compiler_exception(rengine::c_str message) : std::exception(message) {};
};
class invalid_material_exception: public material_compiler_exception {
    public:
        invalid_material_exception(rengine::c_str message) : material_compiler_exception(message) {}
};

struct material_program_data {
    rengine::string vertex;
    rengine::string pixel;
};

struct material_pass_data {
    rengine::string entrypoint;
    rengine::unordered_map<rengine::string, rengine::string> definitions;
    material_program_data program;
};

struct material_data {
    rengine::string name;
    rengine::string author;
    rengine::string version;
    rengine::unordered_map<rengine::string, rengine::string> definitions;
    rengine::string entrypoint;
    material_program_data program;
};

struct program_state {
    rengine::string material_data;
    ryml::Tree tree;
};

static rengine::string g_param_input = "-i";
static rengine::string g_param_output = "-o";
static rengine::string g_param_help = "-h";
static rengine::string g_program_params[] = {
    g_param_input, g_param_output
};

static rengine::hash_map<rengine::string, rengine::string> g_program_args;
static program_state g_program_state = {};

void program_collect_args(rengine::u32 argc, char** argv) {
    rengine::string arg;
    rengine::string path;

    for (rengine::u32 i = 0; i < argc; ++i) {
        arg = argv[i];
        auto match_key = false;

        if (g_param_help == arg) {
            g_program_args[arg] = "help";
            break;
        }

        for (rengine::u32 j = 0; j < _countof(g_program_params); ++j) {
            if (arg != g_program_params[j])
                continue;
            match_key = true;
            break;
        }

        if (!match_key)
            continue;

        if (i == argc - 1)
            continue;

        path = argv[i + 1];
        rengine::io::path_normalize(&path);
        g_program_args[arg] = path;
    }
}
void program_validate_params() {
    if (g_program_args.find(g_param_input) == g_program_args.end())
        throw material_compiler_exception("Required -i argument");
    if (g_program_args.find(g_param_output) == g_program_args.end())
        throw material_compiler_exception("Required -o argument");
}
void program_print_help() {
    std::cout << "*============================================================*" << std::endl;
    std::cout << "| REngine - Material Compiler                                |" << std::endl;
    std::cout << "|------------------------------------------------------------|" << std::endl;
    std::cout << "| Command Example:                                           |" << std::endl;
    std::cout << "| ./material_compiler -i unlit.material -o ./assets/material |" << std::endl;
    std::cout << "|------------------------------------------------------------|" << std::endl;
    std::cout << "| Param | Description                                        |" << std::endl;
    std::cout << "| -i    | Material Description Input Path                    |" << std::endl;
    std::cout << "| -o    | Material Output Path. (Directory where material    |" << std::endl;
    std::cout << "|       | will be saved).                                    |" << std::endl;
    std::cout << "*============================================================*" << std::endl;
}

rengine::string program_get_material_input() {
    const auto it = g_program_args.find_as(g_param_input);
    if (it == g_program_args.end())
        return rengine::string();
    return it->second;
}

void material_read(rengine::c_str path) {
    const auto material_yml = rengine::io::file_read_text(path);
    g_program_state.material_data = material_yml;
    auto data = ryml::substr(
        const_cast<char*>(g_program_state.material_data.c_str())
    );
    g_program_state.tree = ryml::parse_in_place(data);
}

bool material_validate_program(ryml::ConstNodeRef& program_node) {

}

void material_validate() {
    bool has_root_program = false;
    auto& root = g_program_state.tree;
    if (!root.find_child(root.root_id(), "material"))
        throw material_compiler_exception("Required 'material' property");

    const auto material_node = root["material"];
    if (material_node.find_child("name").invalid())
        throw material_compiler_exception("Required 'material.name' property");
    if (material_node.find_child("author").invalid())
        throw material_compiler_exception("Required 'material.author' property");
    if (material_node.find_child("version").invalid())
        throw material_compiler_exception("Required 'material.version' property");
    if (material_node.find_child("entrypoint").invalid())
        throw material_compiler_exception("Required 'material.entrypoint' property");
    if (material_node.find_child("program").valid()) {
        const auto program_node = material_node["program"];
        if (material_validate_program(program_node))
    }

    const auto passes_node = material_node.find_child("passes");
    if (passes_node.invalid())
        throw material_compiler_exception("Required 'material.passes' property");

    if (passes_node.num_children() == 0)
        throw material_compiler_exception("Required at least one pass definition");

    for (const auto& pass_node : passes_node.children()) {
        const auto type = pass_node.type();
        if (!type.has_all(ryml::KEYMAP))
            throw material_compiler_exception("Required 'material.passes' property");
        rengine::string pass_prop(pass_node.key().data());
        pass_prop += ".";

        if (pass_node.find_child("program").invalid())
            throw material_compiler_exception(("Required '"+ pass_prop+"' property").c_str());
    }
}


namespace
{
SlangStage guessStage(const std::string& path)
{
    auto filename = std::filesystem::path(path).filename().string();
    for (auto& c : filename) c = (char)std::tolower(c);

    if (filename.find("vs") != std::string::npos || filename.find("vert") != std::string::npos)
        return SLANG_STAGE_VERTEX;
    if (filename.find("ps") != std::string::npos || filename.find("frag") != std::string::npos)
        return SLANG_STAGE_FRAGMENT;
    if (filename.find("cs") != std::string::npos || filename.find("comp") != std::string::npos)
        return SLANG_STAGE_COMPUTE;
    return SLANG_STAGE_NONE;
}

}

int main(int argc, char** argv)
{
    program_collect_args(argc, argv);
    if (g_program_args.find(g_param_help) != g_program_args.end()) {
        program_print_help();
        return 0;
    }
    ryml::Tree tree;
    material_read(program_get_material_input().c_str());
    material_validate();

    SlangSession* session = spCreateSession(nullptr);
    // SlangCompileRequest* request = spCreateCompileRequest(session);

    // int translationUnit = spAddTranslationUnit(request, SLANG_SOURCE_LANGUAGE_SLANG, "shader");
    // spAddTranslationUnitSourceString(request, translationUnit, shaderPath.c_str(), source.c_str());
    //
    // SlangStage stage = guessStage(shaderPath);
    // if (stage != SLANG_STAGE_NONE)
    //     spAddEntryPoint(request, translationUnit, "main", stage);
    //
    // if (spCompile(request) != SLANG_OK)
    // {
    //     const char* diagnostic = spGetDiagnosticOutput(request);
    //     if (diagnostic)
    //         std::cerr << diagnostic << std::endl;
    //     spDestroyCompileRequest(request);
    //     spDestroySession(session);
    //     rengine::destroy();
    //     return 1;
    // }
    //
    // std::cout << "Slang compilation succeeded" << std::endl;
    //
    // ProgramLayout* program = ProgramLayout::get(request);
    // SlangUInt paramCount = program->getParameterCount();
    //
    // for (SlangUInt i = 0; i < paramCount; ++i)
    // {
    //     auto* param = program->getParameterByIndex(i);
    //     auto* var = param->getVariable();
    //     auto* typeLayout = param->getTypeLayout();
    //     auto* type = typeLayout->getType();
    //
    //     auto kind = type->getKind();
    //     auto category = param->getCategory();
    //
    //     if (kind == TypeReflection::Kind::ConstantBuffer)
    //     {
    //         std::cout << "ConstantBuffer: " << var->getName() << std::endl;
    //     }
    //     else if (category == ParameterCategory::VaryingInput)
    //     {
    //         const char* semantic = param->getSemanticName();
    //         std::cout << "VertexInput: " << var->getName();
    //         if (semantic)
    //             std::cout << " (" << semantic << param->getSemanticIndex() << ")";
    //         std::cout << std::endl;
    //     }
    //     else if (typeLayout->getBindingRangeCount() > 0)
    //     {
    //         for (SlangInt r = 0; r < typeLayout->getBindingRangeCount(); ++r)
    //         {
    //             auto bindingType = typeLayout->getBindingRangeType(r);
    //             if (bindingType == BindingType::Texture)
    //             {
    //                 std::cout << "Texture: " << var->getName() << std::endl;
    //             }
    //         }
    //     }
    // }

    // spDestroyCompileRequest(request);
    spDestroySession(session);
    return 0;
}

