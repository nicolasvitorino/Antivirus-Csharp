using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntivirusApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            btnDeletar.Enabled = false; // Botão começa desabilitado
        }

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            lstResultados.Items.Clear(); // Limpa os resultados anteriores
            lblStatus.Text = "Iniciando busca...";
            btnBuscar.Enabled = false;
            btnDeletar.Enabled = false; // Desabilita o botão enquanto busca

            string[] nomesArquivos = { "script-aula.bat", "script-aula-teste.bat", "execucao-aula.exe", "execucao-aula-teste.exe" };
            int arquivosEncontrados = 0;

            try
            {
                // Obtém todos os drives disponíveis
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady).ToList();

                foreach (var drive in drives)
                {
                    lblStatus.Text = $"Buscando no disco {drive.Name}...";
                    Console.WriteLine($"Buscando no disco {drive.Name}...");

                    var encontrados = await Task.Run(() => BuscarArquivosCompletos(drive.Name, nomesArquivos));

                    if (encontrados.Count > 0)
                    {
                        arquivosEncontrados += encontrados.Count;
                        foreach (var arquivo in encontrados)
                        {
                            lstResultados.Items.Add(arquivo); // Apenas caminhos reais
                            Console.WriteLine($"Arquivo encontrado: {arquivo}");
                        }
                    }
                }

                if (arquivosEncontrados > 0)
                {
                    btnDeletar.Enabled = true; // Habilita o botão "Deletar"
                }

                lblStatus.Text = arquivosEncontrados > 0
                    ? $"Arquivos suspeitos encontrados: {arquivosEncontrados}"
                    : "Nenhum arquivo suspeito encontrado.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante a busca: {ex.Message}");
                lblStatus.Text = "Erro ao realizar a busca.";
            }
            finally
            {
                btnBuscar.Enabled = true; // Habilita o botão novamente
            }
        }

        private static List<string> BuscarArquivosCompletos(string diretorioInicial, string[] nomesArquivos)
        {
            var arquivosEncontrados = new List<string>();
            var pastasParaVerificar = new Queue<string>();
            pastasParaVerificar.Enqueue(diretorioInicial);

            while (pastasParaVerificar.Count > 0)
            {
                var diretorioAtual = pastasParaVerificar.Dequeue();

                try
                {
                    // Busca os arquivos diretamente no diretório atual
                    foreach (var nomeArquivo in nomesArquivos)
                    {
                        var arquivos = Directory.GetFiles(diretorioAtual, nomeArquivo);
                        arquivosEncontrados.AddRange(arquivos);
                    }

                    // Adiciona as subpastas para verificação futura
                    var subPastas = Directory.GetDirectories(diretorioAtual);
                    foreach (var subPasta in subPastas)
                    {
                        pastasParaVerificar.Enqueue(subPasta);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Sem permissão para acessar: {diretorioAtual}");
                }
                catch (PathTooLongException)
                {
                    Console.WriteLine($"Caminho muito longo: {diretorioAtual}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao acessar {diretorioAtual}: {ex.Message}");
                }
            }

            return arquivosEncontrados;
        }

        private void btnDeletar_Click(object sender, EventArgs e)
        {
            if (lstResultados.Items.Count == 0)
            {
                MessageBox.Show("Nenhum arquivo para deletar.", "Ação não permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int deletados = 0;

            foreach (var item in lstResultados.Items)
            {
                string arquivo = item.ToString();

                try
                {
                    // Exclui o arquivo
                    File.Delete(arquivo);
                    deletados++;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Sem permissão para deletar: {arquivo}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Erro ao deletar o arquivo {arquivo}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro inesperado ao deletar o arquivo {arquivo}: {ex.Message}");
                }
            }

            // Atualiza a interface
            lstResultados.Items.Clear();
            btnDeletar.Enabled = false; // Desabilita o botão novamente
            lblStatus.Text = deletados > 0
                ? $"Arquivos deletados: {deletados}"
                : "Nenhum arquivo deletado.";
        }
    }
}
