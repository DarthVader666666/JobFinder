<template>
  <div className="head">
    <h1>Welcome to Job Finder!</h1>
  </div>

  <div className="container">
    <div className="input-box">
      <div className="sources">
        <section v-for="(source, index) in sources" @click="setSearchSource(source.name)" :key="index" :className="isSearchSourceOptionActive(source.name) ? 'active' : ''">
          <img v-bind:src="source.img" :alt="source.name">
        </section>
      </div>
      <CriteriaInput :changeSpeciality="changeSpeciality" :changeArea="changeArea" :findJobs="findJobs"></CriteriaInput>
      <button @click="findJobs()" style="margin-inline: 0.5rem;">Find Job</button>
    </div>

    <div>
      <h3 v-if="loading">Loading...</h3>

      <div v-for="(job, index) in jobs" :key="index" style="display: flex; flex-direction: column;">
        <img v-bind:src="jobs[index].img" width="50px" height="50px">

        <div className="list-box">
          <a v-for="(link, index) in job.links" :href="link.link" :key="index" target="_blank">
            {{link.title}}
          </a>
        </div>        
      </div>
    </div>
    
  </div>
</template>

<script>
import CriteriaInput from './components/CriteriaInput.vue'

export default {
components: {
  CriteriaInput
},

  data() {
    return {
      sources: [
        {
          name:'linkedIn',
          img:'linkedin-logo-large.png'
        },
        {
          name:'rabotaBy',
          img:'rabotaby-logo-large.png'
        },
        {
          name:'devBy',
          img:'devby-logo-large.png'
        },
        {
          name:'pracaBy',
          img:'pracaby-logo-large.png'
        },
        {
          name:'trabajo',
          img:'trabajo-logo-large.png'
        },
        {
          name:'beBee',
          img:'bebee-logo-large.png'
        },
        {
          name: 'joblum',
          img: 'joblum-logo-large.png'
        }
      ],

      jobs: [],
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
      loading: false
    }
  },

  methods: {
    findJobs() {
      var loadedJobs = [];
      this.jobs = [];
      var index = 0;
      
      this.searchSourceOptions.forEach(async searchSourceOption => {
        if(searchSourceOption.active)
        {
          this.loading = true;

          loadedJobs = await fetch(
            'https://localhost:7150/Jobs/GetList',
            {
              body: JSON.stringify({
                speciality: this.speciality,
                area: this.area,
                source: searchSourceOption.name
              }),
              method: 'POST',
              headers: {
                'Content-Type' : 'application/json'            
              }
            }
          ).then(response => response.json());

          this.jobs.push({ img: searchSourceOption.img, links: [] })
          loadedJobs.forEach(job => this.jobs[index].links.push(job));
          
          index++;
          this.loading = false;
        }
      });
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
    flex-direction: column; 
    align-items: center;
  }

  .container {
    display:grid;
    grid-template-columns: 15% 85%;
    max-width: 100%;
  }

  .input-box {
    width:fit-content; 
    display:flex;
    flex-direction: column; 
    align-content: start;
  }

  .list-box {
    flex-direction: row;
    padding-top: 8px;
    padding-bottom: 8px;
    align-content: start;
    gap: 2px;
  }

  a {
      max-height: 18px;
      display: block;
      text-decoration:none;
      color:black;
      padding: 3px;
      margin: 2px;
      background-color: gray;
  }

  a:hover {
    color:lightgray;
    background: #333;
  }

  a:visited {
    color:darkgray;
    font-style: italic;
  }

  h1, h3 {
    color:rgb(180, 29, 29);
    font-weight: bold;
    border-radius:10px;
    width: fit-content;
    padding: 5px;
    text-align: center;
    text-shadow: 0.2rem 0.2rem black;
  }

  .sources {
    display: flex;
    flex-direction: column;
    gap:0.2rem;
    width: fit-content;
    height: fit-content;
    padding: 0;
  }

  .sources section {
    width: 10rem;
    height: 3rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    border: 3px solid black;
    margin:0.1rem;
    background-color: rgb(0, 164, 164);
    border-radius: 10px;
  }

  .sources section img {
    height:95%;
    width:95%;
  }

  section:hover, section.active {
    background-color: aqua;
    cursor: pointer;
  }
</style>
