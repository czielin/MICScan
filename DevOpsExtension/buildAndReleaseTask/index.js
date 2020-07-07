"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    Object.defineProperty(o, k2, { enumerable: true, get: function() { return m[k]; } });
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const tl = require("azure-pipelines-task-lib/task");
const path = __importStar(require("path"));
function run() {
    return __awaiter(this, void 0, void 0, function* () {
        const projectFileName = tl.getInput('ProjectFileName', true);
        const args = [projectFileName, process.env.BUILD_SOURCESDIRECTORY];
        tl.execSync('pip', ['install', 'scikit-learn==0.21.2']);
        // __dirname contains the path where the extension files live
        const scanExePath = path.join(__dirname, '\\scan\\Scan.exe');
        const exitCode = tl.execSync(scanExePath, args).code;
        console.log('Scan complete.');
        if (exitCode == 0) {
            tl.setResult(tl.TaskResult.Succeeded, "MICScan found no flaws.");
        }
        else if (exitCode > 0) {
            const vulnerabilityAction = tl.getInput('VulnerabilityAction', true);
            const taskResult = vulnerabilityAction == "Warn" ? tl.TaskResult.SucceededWithIssues : tl.TaskResult.Failed;
            tl.setResult(taskResult, "MICScan identified " + exitCode + " potential vulnerabilities. See output above for additional information.");
        }
        else if (exitCode < 0) {
            tl.setResult(tl.TaskResult.Failed, "An error occurred during scan. See above output for additional information.");
        }
    });
}
run();
