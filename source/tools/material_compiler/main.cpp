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

class material_compiler: public std::exception {
public:
    material_compiler(rengine::c_str message) : std::exception(message) {};
};

static rengine::string g_param_input = "-i";
static rengine::string g_param_output = "-o";
static rengine::string g_param_help = "-h";
static rengine::string g_program_params[] = {
    g_param_input, g_param_output
};

static rengine::hash_map<rengine::string, rengine::string> g_program_args;

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
        throw material_compiler("Required -i argument");
    if (g_program_args.find(g_param_output) == g_program_args.end())
        throw material_compiler("Required -o argument");
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
    // if (argc < 2)
    // {
    //     std::cerr << "Usage: material_compiler <shader file> <shader stage: vertex | pixel>" << std::endl;
    //     return 1;
    // }
    //
    // const std::string shaderPath = argv[1];
    //
    // std::ifstream file(shaderPath, std::ios::binary);
    // if (!file)
    // {
    //     std::cerr << "Failed to open file: " << shaderPath << std::endl;
    //     return 1;
    // }
    //
    // std::string source((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());

    rengine::init();

    char yml_buf[] = "{foo: 1, bar: [2, 3]}";
    ryml::Tree tree = ryml::parse_in_place(ryml::substr(yml_buf));
    int foo = 0;
    tree["foo"] >> foo;
    std::cout << "Parsed foo=" << foo << std::endl;

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
    rengine::destroy();
    return 0;
}

