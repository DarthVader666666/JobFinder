<template>
  <div className="head">
    <h1>Welcome to Job Finder!</h1>
  </div>
  <div className="container">

    <div className="input-box">
      <div className="sources">
        <section @click="setUrl(linkedIn)" :className="isUrlActive(linkedIn) ? 'active' : ''">
          <img src="./img/linkedin-logo-2.png" alt="linkedIn">
        </section>
        <section @click="setUrl(rabotaBy)" :className="isUrlActive(rabotaBy) ? 'active' : ''">
          <img src="./img/rabotaby-logo-2.png" alt="rabotaBy">
        </section>
        <section @click="setUrl(devBy)" :className="isUrlActive(devBy) ? 'active' : ''">
          <img src="./img/devby-logo-2.png" alt="devBy">
        </section>
      </div>
      <CriteriaInput :changeSpeciality="changeSpeciality" :changeArea="changeArea" :findJobs="findJobs"></CriteriaInput>
      <button @click="findJobs()" style="margin-inline: 0.5rem;">Find Job</button>
    </div>

    <div className="list-box">      
      <h3 v-if="loading">Loading...</h3>
      <div v-for="(el, index) in jobs" :key="index">
        <a :href="el['link']" target="_blank">
          {{el['title']}}
        </a>
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
      linkedIn: 'linkedIn',
      rabotaBy: 'rabotaBy',
      devBy: 'devBy',
      jobs: [],
      urls: [
        {
          name: 'linkedIn',
          path: 'https://www.linkedin.com/search/results/all/?',
          active: false
        },
        {
          name: 'rabotaBy',
          path: 'https://rabota.by/search/vacancy/?',
          active: false
        },
        {
          name: 'devBy',
          path: 'https://jobs.devby.io/?',
          active: false
        }
      ],
      speciality: '',
      area: '',
      loading: false
    }
  },

  methods: {
    async findJobs() {
      var loadedJobs = [];
      this.jobs = [];
      
      this.urls.forEach(async url => {
        if(url.active)
        {
          this.loading = true;

          loadedJobs = await fetch(
            'https://localhost:7150/Jobs/GetList',
            {
              body: JSON.stringify({
                url: url.path,
                speciality: this.speciality,
                area: this.area,
                source: url.name
              }),
              method: 'POST',
              headers: {
                'Content-Type' : 'application/json'            
              }
            }
          ).then(response => response.json());

          loadedJobs.forEach(job => this.jobs.push(job));          
        }

        this.loading = false
      });
    },

    changeSpeciality(value) {
      this.speciality = String(value).trim();
    },

    changeArea(value) {
      this.area = String(value).trim();
    },

    setUrl(value) {
      this.urls.forEach(url => {
        if (url['name'] === value) {
          url['active'] = !url['active'];
          return;
        }
      })
    },

    isUrlActive(value) {
      const active = this.urls.find(x => x['name'] == value)['active'];      
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
    flex-direction: row;
    max-width: 100%;
  }

  .input-box {
    width:fit-content; 
    flex-direction: column; 
    align-content: start;
  }

  .list-box {
    max-width: 83%; 
    flex-direction: row;
    padding-top: 8px;
    justify-content: start;
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
