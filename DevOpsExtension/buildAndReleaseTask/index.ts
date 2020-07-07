import tl = require('azure-pipelines-task-lib/task');
import * as path from 'path';

async function run() {
    const projectFileName = <string>tl.getInput('ProjectFileName', true);
    const args: string[] = [projectFileName, <string>process.env.BUILD_SOURCESDIRECTORY];
    tl.execSync('pip', ['install', 'scikit-learn==0.21.2']);
    // __dirname contains the path where the extension files live
    const scanExePath = path.join(__dirname, '\\scan\\Scan.exe');
    const exitCode = tl.execSync(scanExePath, args).code;
    console.log('Scan complete.');
    if (exitCode == 0) {
        tl.setResult(tl.TaskResult.Succeeded, "MICScan found no flaws.");
    } else if (exitCode > 0) {
        const vulnerabilityAction = <string>tl.getInput('VulnerabilityAction', true);
        const taskResult = vulnerabilityAction == "Warn" ? tl.TaskResult.SucceededWithIssues : tl.TaskResult.Failed;
        tl.setResult(taskResult, "MICScan identified " + exitCode + " potential vulnerabilities. See output above for additional information.");
    } else if (exitCode < 0) {
        tl.setResult(tl.TaskResult.Failed, "An error occurred during scan. See above output for additional information.");
    }
}

run();