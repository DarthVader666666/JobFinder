<template>

  <div className="head">
    <h3>Welcome to Job Finder!</h3>
    <button @click="showResources" className="menu-button">Resources</button>
  </div>

  <div className="sources" v-show="show">
    <section v-for="(source, index) in sources" @click="setSearchSource(source.name)" :key="index" :className="isSearchSourceOptionActive(source.name) ? 'active' : ''">
      <img v-bind:src="source.img" :alt="source.name">
    </section>
  </div>

  <CriteriaInput :changeSpeciality="changeSpeciality"
                 :changeArea="changeArea"
                 :findJobs="findJobs"></CriteriaInput>

  <div>
    <h3 v-if="loading">Loading...</h3>
    <div style="display: flex; flex-direction: column;">
      <div v-for="(response, index) in responses" :key="index" style="display: flex; flex-direction: column;">
        <div className="list-box">
          <a :href="response.sourceUrl" target="_blank">
           <img v-bind:src="response.img" width="50px" height="50px">
          </a>
        </div>  
        <div >
          <a v-for="(job, index) in response.jobs" :key="index" className="jobLink" :href="job.link" target="_blank">
            <span style="font-weight: bold;">{{job.title}}</span>
            <span>{{job.salary}}</span>
          </a>
        </div>          
      </div>
    </div>
  </div>

</template>

  <script>
    import CriteriaInput from './components/CriteriaInput.vue'
    export default {
      created() {
        this.url = import.meta.env.VITE_API_URL;
      },
      components: {
        CriteriaInput
      },
      data() {
        return {
          sources: [
            {
              name: 'linkedIn',
              img: 'linkedin-logo-large.png'
            },
            {
              name: 'rabotaBy',
              img: 'rabotaby-logo-large.png'
            },
            {
              name: 'devBy',
              img: 'devby-logo-large.png'
            },
            {
              name: 'pracaBy',
              img: 'pracaby-logo-large.png'
            },
            {
              name: 'trabajo',
              img: 'trabajo-logo-large.png'
            },
            {
              name: 'beBee',
              img: 'bebee-logo-large.png'
            },
            {
              name: 'joblum',
              img: 'joblum-logo-large.png'
            }
          ],
          responses: [],
          searchSourceOptions: [
            {
              img: 'linkedin-logo-small.png',
              name: 'linkedIn',
              active: false
            },
            {
              img: 'rabotaby-logo-small.png',
              name: 'rabotaBy',
              active: false
            },
            {
              img: 'devby-logo-small.png',
              name: 'devBy',
              active: false
            },
            {
              img: 'pracaby-logo-small.png',
              name: 'pracaBy',
              active: false
            },
            {
              img: 'trabajo-logo-small.png',
              name: 'trabajo',
              active: false
            },
            {
              img: 'bebee-logo-small.png',
              name: 'beBee',
              active: false
            },
            {
              img: 'joblum-logo-small.png',
              name: 'joblum',
              active: false
            }
          ],
          speciality: '',
          area: '',
          loading: false,
          url: null,
          show: false
        }
      },
      methods: {
        async findJobs() {
          this.show = false;
          var loadedResponses = [];
          this.responses = [];
          var bodyValue = {
                    speciality: this.speciality,
                    area: this.area,
                    sources: [] 
                  };

          this.searchSourceOptions.forEach(option => { if(option.active) bodyValue.sources.push(option.name) })                  

          this.loading = true;
              loadedResponses = await fetch(
                `${this.url}/Jobs/GetList`,
                {
                  body: JSON.stringify(bodyValue),
                  method: 'POST',
                  headers: {
                    'Content-Type': 'application/json'
                  }
                }
              ).then(response => response.json());

              this.searchSourceOptions.forEach(option => {
                  if(option.active)
                  {
                    this.responses.push(
                      { 
                        img: option.img, 
                        sourceName: option.name, 
                        sourceUrl: loadedResponses.find(lr => lr.sourceName === option.name).sourceUrl,
                        jobs: loadedResponses.find(lr => lr.sourceName === option.name).jobs
                      })
                  }
              });

              this.loading = false;
        },
        changeSpeciality(value) {
          this.speciality = String(value).trim();
        },
        changeArea(value) {
          this.area = String(value).trim();
        },
        setSearchSource(value) {
          this.searchSourceOptions.forEach(searchSourceOption => {
            if (searchSourceOption['name'] === value) {
              searchSourceOption['active'] = !searchSourceOption['active'];
              return;
            }
          })
        },
        isSearchSourceOptionActive(value) {
          const active = this.searchSourceOptions.find(x => x['name'] == value)['active'];
          return active;
        },
        showResources() {
          this.show = !this.show;
        }
      }
    }
  </script>

  <style scoped>
    div {
      display: flex;
      flex-wrap: wrap;
    }

    .head {
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
    }

    .list-box {
      flex-direction: row;
      padding-top: 8px;
      padding-bottom: 8px;
      align-content: start;
      gap: 2px;
    }

    .jobLink {
      max-height: 18px;
      display: block;
      text-decoration: none;
      color: black;
      padding: 3px;
      margin: 2px;
      background-color: gray;
    }

    .jobLink:hover {
      color: lightgray;
      background: #333;
    }
    .jobLink:visited {
      color: darkgray;
      font-style: italic;
    }

    h1, h3 {
      color: rgb(180, 29, 29);
      font-weight: bold;
      border-radius: 10px;
      width: fit-content;
      padding: 5px;
      text-align: center;
      text-shadow: 0.1rem 0.1rem black;
    }

    .sources {
      display: flex;
      flex-direction: column;
      gap: 0.2rem;
      width: fit-content;
      height: fit-content;
      padding: 10px;
      float: inline-end;
    }

      .sources section {
        width: 10rem;
        height: 3rem;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        border: 3px solid black;
        margin: 0.1rem;
        background-color: rgb(0, 164, 164);
        border-radius: 10px;
      }

        .sources section img {
          height: 95%;
          width: 95%;
        }

    section.active {
      background-color: aqua;
      cursor: pointer;
    }

    section:hover {
      cursor: pointer;
    }

    .menu-button {
      height: 50px;
      background-color: rgb(16, 16, 181);
      color: rgb(224, 223, 242);
      border-radius: 15%;
      font-weight: bold;
      font-size: 1rem;
    }

    .menu-button:hover {
      cursor: pointer;
    }

    .menu-button:active {
      background-color: rgba(16, 16, 181, 0.658);;
    }
  </style>
